using System.Threading.Tasks;

namespace Horarium.Builders.Recurrent
{
    public interface IRecurrentJobBuilder
    {
        /// <summary>
        /// Add special key(unique identity for recurrent job), default is class name
        /// </summary>
        /// <param name="jobKey"></param>
        /// <returns></returns>
        IRecurrentJobBuilder WithKey(string jobKey);

        /// <summary>
        /// Run current job
        /// </summary>
        /// <returns></returns>
        Task<string> Schedule();
    }
}