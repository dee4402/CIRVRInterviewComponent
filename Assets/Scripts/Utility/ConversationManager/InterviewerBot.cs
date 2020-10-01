using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;


namespace Cirvr.ConversationManager
{
    public sealed class InterviewerBot : IBot
    {
        public static IEnumerable<string> userProps = typeof(UserState).GetProperties().Select<PropertyInfo, string>(x => x.Name);
        private static readonly InterviewerBot instance = new InterviewerBot();
        public bool dialogIsSet = false;

        private string[] interviewerNames = { "Adrianna", "Belle", "Krista", "Delphine", "Jean", "Corrine" };

        static InterviewerBot() { }

        private InterviewerBot() { }

        public void SetPropValue(ConversationContext context)
        {
            Dialog currentDialog = context.GetCurrentDialog();
            UserState userState = context.UserState;

            // Save any important user state info we received 
            // @todo The property we save the entity in to should depend on the dialog we're currently on
            if(currentDialog.requiredParams != null && currentDialog.requiredParams.Count() > 0)
            {
               
            }
        }

      

        public string MakeSentence(Dialog dialog, UserState user, string replacementSent = null)
        {
            string text = dialog.DialogText;
            string pattern = @"{{(?<param>\w+)}}";
            MatchEvaluator evaluator = new MatchEvaluator(match =>
            {
                GroupCollection groups = match.Groups;
                if (dialog.staticParams.Contains(groups["param"].Value))
                {
                    return StaticReplace(groups["param"].Value);
                } 

                if(dialog.dynamicParams.Contains(groups["param"].Value))
                {
                    return user.GetPropertyValue<string>(groups["param"].Value);
                }

                // This returns an empty string
                return groups["param"].Value;
                
                // We should also throw an exception

            });

            if (replacementSent == null)
            {
                return Regex.Replace(text, pattern, evaluator, RegexOptions.IgnorePatternWhitespace);
            }
            
            return Regex.Replace(replacementSent, pattern, evaluator, RegexOptions.IgnorePatternWhitespace);
        }

        public string StaticReplace(string toReplace)
        {
            System.Random random = new System.Random();
            switch (toReplace)
            {
                case "interviewerName":
                    return interviewerNames[random.Next(0,5)];
                case "greetingTime":
                    DateTime localDate = DateTime.Now;
                    int hour = localDate.Hour;

                    if (hour <= 11)
                    {
                        return "morning";
                    }
                    else if (hour < 16)
                    {
                        return "afternoon";
                    }
                    else
                    {
                        return "evening";
                    }
                default:
                    return "evening";
            }
        }

        public static InterviewerBot Instance
        {
            get
            {
                return instance;
            }
        }

        string AttributeMap(string entity)
        {
            switch (entity)
            {
                case "builtin.personName":
                    return "personName";
                case "builtin.number":
                    return "hoursPerWeek";
                default:
                    return entity;
            }
        }

        public Dialog FlowDialog(string dialogId, ConversationContext context)
        {
            context.SetDialog(dialogId);
            return context.GetCurrentDialog();
        }

        public string GetFirstQuestion(ConversationContext context)
        {
            dialogIsSet = true;
            Dialog d = context.GetCurrentDialog();
            return MakeSentence(d, context.UserState);
        }
    }
}