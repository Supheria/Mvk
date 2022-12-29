using MvkServer.Util;
using MvkServer.World;
using System.Collections.Generic;

namespace MvkServer.Entity.AI
{
    /// <summary>
    /// Объект списка возможных задач искусственного интеллекта моба
    /// </summary>
    public class EntityAITasks
    {
        /// <summary>
        /// Список всех задача
        /// </summary>
        private List<EntityAITaskEntry> taskEntries = new List<EntityAITaskEntry>();
        /// <summary>
        /// Список выполняемых задач
        /// </summary>
        private List<EntityAITaskEntry> executingTaskEntries = new List<EntityAITaskEntry>();
        /// <summary>
        /// Счётчик тиков
        /// </summary>
        private int tickCount;
        /// <summary>
        /// Объект мира
        /// </summary>
        private readonly WorldBase world;
        /// <summary>
        /// Объект для лога
        /// </summary>
        private readonly Profiler profiler;

        public EntityAITasks(WorldBase world)
        {
            this.world = world;
            this.profiler = world.profiler;
        }

        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="priority">Приоритет задачи</param>
        /// <param name="action">Объект задачи</param>
        public void AddTask(int priority, EntityAIBase task) => taskEntries.Add(new EntityAITaskEntry(priority, task));
        /// <summary>
        /// Удалить задачу, проверка по ссылке, задача должна быть одним и тем же объектом
        /// </summary>
        public void RemoveTask(EntityAIBase task)
        {
            int index = -1;
            for (int i = 0; i < taskEntries.Count; i++)
            {
                if (taskEntries[i].task == task)
                {
                    index = i;
                    break;
                }
            }
            if (index != 1)
            {
                EntityAITaskEntry taskEntry = taskEntries[index];

                if (executingTaskEntries.Contains(taskEntry))
                {
                    taskEntry.task.ResetTask();
                    executingTaskEntries.Remove(taskEntry);
                }
                taskEntries.RemoveAt(index);
            }
        }

        /// <summary>
        /// Обновить задачи
        /// </summary>
        public void OnUpdateTasks()
        {
            int i;
            profiler.StartSection("GoalSetup");
            // Выборка запуска задачи
            EntityAITaskEntry taskEntry;

            if (tickCount++ % 3 == 0)
            {
                // Каждый третий такт мы тут
                for (i = 0; i < taskEntries.Count; i++)
                {
                    taskEntry = taskEntries[i];
                    if (executingTaskEntries.Contains(taskEntry))
                    {
                        // эта задача выполняется
                        if (CanUse(taskEntry) && taskEntry.task.ContinueExecuting()) continue;
                        // Задачу надо прервать
                        taskEntry.task.ResetTask();
                        executingTaskEntries.Remove(taskEntry);
                    }
                    if (CanUse(taskEntry) && taskEntry.task.ShouldExecute())
                    {
                        // Начинаем выполнять задачу
                        taskEntry.task.StartExecuting();
                        executingTaskEntries.Add(taskEntry);
                    }
                }
            }
            else if (executingTaskEntries.Count > 0)
            {
                // Тут проверка на окночание задачи
                List<int> indexs = new List<int>();
                for (i = 0; i < executingTaskEntries.Count; i++)
                {
                    taskEntry = executingTaskEntries[i];
                    if (!taskEntry.task.ContinueExecuting())
                    {
                        taskEntry.task.ResetTask();
                        indexs.Add(i);
                    }
                }
                // Удалить в списке задача
                for (i = indexs.Count - 1; i >= 0; i--)
                {
                    executingTaskEntries.RemoveAt(indexs[i]);
                }
            }

            profiler.EndStartSection("GoalTick");
            // Выполнение активной задачи
            for (i = 0; i < executingTaskEntries.Count; i++)
            {
                executingTaskEntries[i].task.UpdateTask();
            }
            profiler.EndSection();
        }

        /// <summary>
        /// Определите, может ли конкретная задача быть выполнена, что означает, 
        /// что все запущенные задачи с более высоким приоритетом совместимы с ней 
        /// или все задачи с более низким приоритетом могут быть прерваны.
        /// </summary>
        private bool CanUse(EntityAITaskEntry inTaskEntry)
        {
            EntityAITaskEntry taskEntry;
            for (int i = 0; i < taskEntries.Count; i++)
            {
                taskEntry = taskEntries[i];
                if (taskEntry != inTaskEntry)
                {
                    if (inTaskEntry.priority >= taskEntry.priority)
                    {
                        if (!((inTaskEntry.task.GetMutexBits() & taskEntry.task.GetMutexBits()) == 0)
                            && executingTaskEntries.Contains(taskEntry)) return false;
                    }
                    else if (!taskEntry.task.IsInterruptible()
                        && executingTaskEntries.Contains(taskEntry)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Статус задачи с приоритетом
        /// </summary>
        class EntityAITaskEntry
        {
            public EntityAIBase task;
            public int priority;

            public EntityAITaskEntry(int priority, EntityAIBase task)
            {
                this.priority = priority;
                this.task = task;
            }
        }
    }
}
