using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toggl.Interfaces
{
    public interface ITaskService
    {
		Task<Task> Get(int id);

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#post_tasks
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<Task> Add(Task t);

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#put_tasks
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Task<Task> Edit(Task t);

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#del_tasks
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<bool> Delete(int id);
    }
}