namespace Cirvr.ConversationManager
{

    public enum Action { OUT_OF_SCOPE, ANXIETY_TRIGGER, MOVE_ON, CONTINUE, TIME_OUT, REPEAT, END_INTERVIEW, CHILD, BAD_ANSWER, NONE, INTENT };
    interface IBot
    {

        /// <summary>
        /// Parse azure analytics data returned from interviewee's response and 
        /// respond accordingly.
        /// </summary>
        /// <returns>Action determined from processing the interviewee response.</returns>
        //string GetResponse(ConversationContext context);

    }
}

