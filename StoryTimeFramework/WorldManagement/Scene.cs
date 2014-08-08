﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoryTimeFramework.Entities;
using StoryTimeCore.Contexts.Interfaces;
using StoryTimeCore.Exceptions;
using StoryTimeFramework.DataStructures;
using StoryTimeFramework.Entities.Actors;
using StoryTimeFramework.Entities.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using StoryTimeCore.Input.Time;
using StoryTimeFramework.WorldManagement.Manageables;
using StoryTimeFramework.Entities.Controllers;
using System.Reflection;
using StoryTimeCore.DataStructures;
using StoryTimeSceneGraph;
using StoryTimeCore.Resources.Graphic;
using StoryTimeCore.General;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace StoryTimeFramework.WorldManagement
{
    /// <summary>
    /// Singleton class that represents the world.
    /// This class is the base of the framework, and will contain all the information about the elements of the world.
    /// The purpose of this class is to process the pipeline of the game world and to contain queryable information about the world.
    /// </summary>
    public class Scene
    {
        private class OrderedActor : IBoundingBoxable
        {
            private BaseActor _actor;

            public OrderedActor(BaseActor actor)
            {
                _actor = actor;
            }

            public OrderedActor(BaseActor actor, int zOrder)
                : this(actor)
            {
                ZOrder = zOrder;
            }

            public Rectanglef BoundingBox { get { return _actor.BoundingBox; } }
            public BaseActor Actor { get { return _actor; } }
            public int ZOrder { get; set; }

        }

        // The list of the many World Entities in the scene.
        private List<BaseActor> _baseActors;
        private Quadtree<BaseActor> _actorsTree;
        private ICamera _activeCamera;
        private Dictionary<BaseActor, OrderedActor> _actorsDictionary;
        private int _nextIndex;

        private WorldTime _currentTime;

        public string SceneName { get; set; }
        public ICamera Camera { get { return _activeCamera; } }
        public IEnumerable<BaseActor> Actors { get { return _baseActors; } }
        public World World { get; private set; }

        public Scene()
        {
            _baseActors = new List<BaseActor>();
            _actorsTree = new Quadtree<BaseActor>();
            _actorsDictionary = new Dictionary<BaseActor, OrderedActor>();
            _nextIndex = 0;
            _activeCamera = new Camera() { Viewport = new Viewport(0, 0, 1280, 720) }; //1280
            World = new World(Vector2.Zero);
        }

        public Scene(Vector2 gravity)
            : this()
        {
            World.Gravity = gravity;
        }

        public void Render(IGraphicsContext graphicsContext)
        {
            //clear graphics device
            if (_activeCamera == null) return;
            //set viewport
            Viewport vp = _activeCamera.Viewport;
            graphicsContext.SetSceneDimensions(vp.Width, vp.Height);
            Rectanglef renderingViewport = new Rectanglef(vp.X, vp.Y, vp.Height, vp.Width);
            IEnumerable<BaseActor> enumActors = GetRenderablesIn(renderingViewport);
            IRenderer renderer = graphicsContext.GetRenderer();
            foreach (BaseActor ba in enumActors)
            {
                renderer.TranslationTransformation += ba.Body.Position;
                ba.RenderableAsset.Render(renderer);
                renderer.TranslationTransformation -= ba.Body.Position;
            }
        }

        public void Update(WorldTime WTime)
        {
            _currentTime = WTime;
            foreach (BaseActor ba in _baseActors)
            {
                ba.TimeElapse(_currentTime);
            }
        }

        public void AddActor(BaseActor ba)
        {
            if (_baseActors.Contains(ba)) return;

            _baseActors.Add(ba);
            OrderedActor oa = new OrderedActor(ba, _nextIndex);
            _nextIndex++;
            AddOrderedAsset(oa);
        }

        public void RemoveActor(BaseActor ba)
        {
            if (!_baseActors.Contains(ba)) return;

            _baseActors.Remove(ba);
            OrderedActor oa;
            if (_actorsDictionary.TryGetValue(ba, out oa))
            {
                RemoveOrderedActor(oa);
                ReOrderActors();
            }
        }

        public List<BaseActor> Intersect(Vector2 point)
        {
            List<BaseActor> actors = _actorsTree.Intersect(point);
            List<OrderedActor> orderActors = new List<OrderedActor>();
            foreach (BaseActor actor in actors)
            {
                orderActors.Add(_actorsDictionary[actor]);
            }
            return
                orderActors
                .OrderByDescending((oActor) => oActor.ZOrder)
                .Select((oActor) => oActor.Actor)
                .ToList();
        }

        private void AddOrderedAsset(OrderedActor orderedAsset)
        {
            _actorsDictionary.Add(orderedAsset.Actor, orderedAsset);
            orderedAsset.Actor.OnBoundingBoxChanges += RenderableAssetBoundsChange;
            _actorsTree.Add(orderedAsset.Actor);
        }

        private void RemoveOrderedActor(OrderedActor orderedAsset)
        {
            bool removed = _actorsTree.Remove(orderedAsset.Actor);
            if (removed)
            {
                _actorsDictionary.Remove(orderedAsset.Actor);
                orderedAsset.Actor.OnBoundingBoxChanges -= RenderableAssetBoundsChange;
            }
        }

        private IEnumerable<BaseActor> GetRenderablesIn(Rectanglef renderingViewport)
        {
            List<BaseActor> actors = new List<BaseActor>();
            Action<BaseActor> renderHitAction = (actor) => actors.Add(actor);
            _actorsTree.Query(renderingViewport, renderHitAction);
            IEnumerable<BaseActor> enumActors = actors.OrderBy(actor => ZIndexOrder(actor));
            return enumActors;
        }

        private int ZIndexOrder(BaseActor actor)
        {
            return _actorsDictionary[actor].ZOrder;
        }

        private void RenderableAssetBoundsChange(BaseActor actor)
        {
            OrderedActor oa;
            if (_actorsDictionary.TryGetValue(actor, out oa))
            {
                RemoveOrderedActor(oa);
                AddOrderedAsset(oa);
            }
        }

        private void ReOrderActors()
        {
            _nextIndex = 0;
            foreach (OrderedActor oa in _actorsDictionary.Values)
            {
                oa.ZOrder = _nextIndex;
                _nextIndex++;
            }

        }
    }
}
