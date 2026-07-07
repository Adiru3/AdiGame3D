using System;
using Engine.Core.Entities;
using Engine.Core.Scene;
using Engine.Editor.Rendering;

namespace Engine.Editor.Editor
{
    public enum EditorMode
    {
        Place  = 0,
        Select = 1,
        Delete = 2,
    }

    /// <summary>
    /// Машина состояний редактора: управляет режимами Place/Select/Delete
    /// и обрабатывает клики и наведение мыши.
    /// </summary>
    public class EditorStateMachine
    {
        private SceneManager  _scene;
        private EditorCamera  _camera;

        // ─── Состояние ────────────────────────────────────────────────────

        public EditorMode   CurrentMode       { get; private set; } = EditorMode.Place;
        public EntityType   SelectedBlockType { get; set; }         = EntityType.Block;
        public Entity       SelectedEntity    { get; private set; }
        public Vec3         PreviewPosition   { get; private set; }

        public int ViewWidth  { get; set; } = 800;
        public int ViewHeight { get; set; } = 600;

        // ─── События ─────────────────────────────────────────────────────

        public event Action<Entity>  EntitySelected;
        public event Action          SelectionCleared;
        public event Action<Entity>  EntityPlaced;
        public event Action<Entity>  EntityDeleted;
        public event Action          SceneModified;

        // ─── Undo стек (простая история действий) ────────────────────────

        private System.Collections.Generic.Stack<UndoAction> _undoStack
            = new System.Collections.Generic.Stack<UndoAction>();
        private System.Collections.Generic.Stack<UndoAction> _redoStack
            = new System.Collections.Generic.Stack<UndoAction>();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public EditorStateMachine(SceneManager scene, EditorCamera camera)
        {
            _scene  = scene;
            _camera = camera;
        }

        // ─── Смена режима ─────────────────────────────────────────────────

        public void SetMode(EditorMode mode)
        {
            CurrentMode    = mode;
            PreviewPosition = null;
            if (mode != EditorMode.Select)
            {
                SelectedEntity = null;
                SelectionCleared?.Invoke();
            }
        }

        // ─── Обработка мыши ──────────────────────────────────────────────

        /// <summary>Вызывается при движении мыши (для превью).</summary>
        public void OnMouseMove(int x, int y)
        {
            if (CurrentMode != EditorMode.Place) return;

            var ray = _camera.ScreenPointToRay(x, y, ViewWidth, ViewHeight);
            Vec3 pos;
            if (RayCaster.GetPlacementPosition(ray, _scene, out pos))
                PreviewPosition = pos;
            else
                PreviewPosition = null;
        }

        /// <summary>Вызывается при клике левой кнопкой мыши.</summary>
        public void OnLeftClick(int x, int y)
        {
            var ray = _camera.ScreenPointToRay(x, y, ViewWidth, ViewHeight);

            switch (CurrentMode)
            {
                case EditorMode.Place:   HandlePlace(ray);  break;
                case EditorMode.Select:  HandleSelect(ray); break;
                case EditorMode.Delete:  HandleDelete(ray); break;
            }
        }

        // ─── Режим Place ─────────────────────────────────────────────────

        private void HandlePlace(Rendering.Ray ray)
        {
            Vec3 pos;
            if (!RayCaster.GetPlacementPosition(ray, _scene, out pos)) return;

            // Не ставим поверх существующего блока
            if (_scene.HasEntityAt(pos)) return;

            var e = _scene.AddEntity(SelectedBlockType, pos);

            _undoStack.Push(new UndoAction { Type = UndoType.Place, Entity = e });
            _redoStack.Clear();

            EntityPlaced?.Invoke(e);
            SceneModified?.Invoke();
        }

        // ─── Режим Select ────────────────────────────────────────────────

        private void HandleSelect(Rendering.Ray ray)
        {
            var hit = RayCaster.PickEntity(ray, _scene);
            if (hit != null)
            {
                SelectedEntity = hit;
                EntitySelected?.Invoke(hit);
            }
            else
            {
                SelectedEntity = null;
                SelectionCleared?.Invoke();
            }
        }

        // ─── Режим Delete ────────────────────────────────────────────────

        private void HandleDelete(Rendering.Ray ray)
        {
            var hit = RayCaster.PickEntity(ray, _scene);
            if (hit == null) return;

            // Нельзя удалять спавн игрока
            if (hit.Type == EntityType.PlayerSpawn) return;

            var clone = hit.Clone();
            clone.Id = hit.Id;  // сохраняем оригинальный Id для undo

            _scene.RemoveEntity(hit.Id);

            _undoStack.Push(new UndoAction { Type = UndoType.Delete, Entity = clone });
            _redoStack.Clear();

            if (SelectedEntity?.Id == hit.Id)
            {
                SelectedEntity = null;
                SelectionCleared?.Invoke();
            }

            EntityDeleted?.Invoke(hit);
            SceneModified?.Invoke();
        }

        // ─── Undo / Redo ─────────────────────────────────────────────────

        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            var action = _undoStack.Pop();

            switch (action.Type)
            {
                case UndoType.Place:
                    _scene.RemoveEntity(action.Entity.Id);
                    break;
                case UndoType.Delete:
                    _scene.AddEntityRaw(action.Entity);
                    break;
            }

            _redoStack.Push(action);
            SceneModified?.Invoke();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            var action = _redoStack.Pop();

            switch (action.Type)
            {
                case UndoType.Place:
                    _scene.AddEntityRaw(action.Entity);
                    break;
                case UndoType.Delete:
                    _scene.RemoveEntity(action.Entity.Id);
                    break;
            }

            _undoStack.Push(action);
            SceneModified?.Invoke();
        }
    }

    // ─── Undo история ────────────────────────────────────────────────────

    public enum UndoType { Place, Delete }

    public class UndoAction
    {
        public UndoType UndoType { get; set; }
        public Engine.Core.Entities.Entity Entity { get; set; }
        public UndoType Type { get; set; }
    }
}
