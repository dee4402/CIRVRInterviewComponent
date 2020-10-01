namespace Cirvr.ConversationManager {
    
    public interface IFilter<T> {

        /// <summary>
        /// Filter an Interviewee response object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Whatever type is required by the filter implementation</returns>
        T ApplyFilter(ConversationContext context);

    }
}

