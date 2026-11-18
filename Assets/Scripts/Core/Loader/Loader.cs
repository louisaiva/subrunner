using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable] public abstract class Loader
{
    protected abstract bool Load(string id/*, out List<LoadTask> sub_tasks*/);
    protected abstract bool Unload(string id/*, out List<LoadTask> sub_tasks*/);


    // TASKS TYPES
    [Serializable] public abstract class LoadTask
    {
        // type
        [SerializeField] protected TaskType type = TaskType.Default;
        public TaskType Type { get => type; }

        // state
        [SerializeField] protected TaskState state = TaskState.Waiting;
        public TaskState State { get => state; }

        // done
        public bool IsDone { get => state == TaskState.Done; }
        public System.Action<LoadTask> OnDone;

        // ids
        [SerializeField] protected List<string> ids = new List<string>();

        // sub tasks
        [SerializeField] protected List<LoadTask> sub_tasks = new List<LoadTask>();


        ///
        //
        /// CONSTRUCTOR & METHODS
        //
        ///

        public LoadTask(string[] ids)
        {
            this.ids.Clear();
            this.ids.AddRange(ids);
        }

        // done & clear
        public void Done()
        {
            state = TaskState.Done;
            OnDone?.Invoke(this);
        }
        public void Clear()
        {
            ids.Clear();
            state = TaskState.Waiting;
        }

        // we do this to prevent the Loader from exposing Load(id) & Unload(id)
        protected bool Load(string id, Loader loader/*, out List<LoadTask> sub_tasks*/) { return loader.Load(id/*, out List<LoadTask> sub_tasks*/); }
        protected bool Unload(string id, Loader loader/*, out List<LoadTask> sub_tasks*/) { return loader.Unload(id/*, out List<LoadTask> sub_tasks*/); }

        // run
        List<LoadTask> new_sub_tasks = new List<LoadTask>();
        public void Run(Loader loader)
        {
            if (type == TaskType.Default) { Debug.LogError($"(Loader.LoadTask) Task of type {this.GetType().Name} can't run bc it is still TaskType.Default. Assign a task type in the constructor of {this.GetType().Name}"); return; }

            state = TaskState.Running;
            new_sub_tasks.Clear();

            // we get the intersection of the chunks to load with the ones that are still waiting for load
            for (int i = ids.Count - 1; i >= 0; i--)
            {
                string id = ids[i];

                if (Type == TaskType.Load)
                {
                    if (!LoadedTheIDWithSuccess(id, loader /*, out new_sub_tasks*/)) { continue; }
                }
                else if (Type == TaskType.Unload)
                {
                    if (!UnloadedTheIDWithSuccess(id, loader /*, out new_sub_tasks*/)) { continue; }
                }
                
                // we successfully loaded the chunk, we remove it from our list
                ids.RemoveAt(i);

                // if we gathered new sub tasks, we add them
                if (new_sub_tasks.Count <= 0) { continue; }
                sub_tasks.AddRange(new_sub_tasks);
            }
            if (ids.Count > 0) { state = TaskState.Waiting; return; } // if we still have some chunks to load, we rewait

            // we clear the done sub tasks
            for (int i = sub_tasks.Count-1; i >= 0; i--)
            {
                if (!sub_tasks[i].IsDone) { continue; }
                sub_tasks.RemoveAt(i);
            }
            if (sub_tasks.Count > 0) { state = TaskState.WaitingSubTasks; return; }

            // else we are done !!
            Done();
        }
        public virtual bool LoadedTheIDWithSuccess(string id, Loader loader/*, out List<LoadTask> sub_tasks*/)
        {
            if (!Load(id, loader)) { return false; }
            return true;
        }
        public virtual bool UnloadedTheIDWithSuccess(string id, Loader loader/*, out List<LoadTask> sub_tasks*/)
        {
            if (!Unload(id, loader)) { return false; }
            return true;
        }
    }

}
public enum TaskState
{
    Waiting,
    Running,
    WaitingSubTasks,
    Done
}
public enum TaskType
{
    Default,
    Load,
    Unload
}