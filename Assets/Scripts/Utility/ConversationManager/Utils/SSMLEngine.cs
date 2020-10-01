using System.Net.Mime;
using System.IO;
using System;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Cirvr.ConversationManager {

    public class SSMLEngine
    {

        public static string RenderSSML(SSMLSettings attributes,float stressLevel)
        {
            string voiceType = "Neutral";
            if (stressLevel < .35)
            {
                voiceType = "Newscast";
            }
            else if (stressLevel <= .6)
            {
                voiceType = "Chat";
            }
            else
                voiceType = "sentiment";

            // Make new SSMLObject from the config option passed in
            SSMLObject ssmlSettings = new SSMLObject(attributes.text, attributes.voice,voiceType);
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("mstts", "https://www.w3.org/2001/mstts");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SSMLObject));
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ssmlSettings,namespaces);
                return @textWriter.ToString();
            }
        }

        /// <summary>
        /// Represents the SSML Schema
        /// </summary>
        [XmlRootAttribute("speak", Namespace = "http://www.w3.org/2001/10/synthesis", IsNullable = false)]
        public class SSMLObject
        {
            [XmlAttribute("xml:lang")]
            public string lang = "en-US";

            [XmlAttribute]
            public string version = "1.0";

            [XmlElement(ElementName = "voice")]
            public Voice voice;

            public SSMLObject(string text, string voice,string voiceType)
            {

                this.voice = new Voice(text, voice,voiceType);
            }

            public SSMLObject() { }
        }
        [XmlType(Namespace = "https://www.w3.org/2001/mstts")]


        public class Voice
        {          

            [XmlAttribute]
            public string name = "Microsoft Server Speech Text to Speech Voice (en-US, JessaNeural)";
            
            [XmlElement(ElementName = "voice")]
            public Voice voice;

            [XmlElement(ElementName = "express-as")]
            public Express express;

            public Voice(string text, string voice,string voiceType)
            {
                this.express = new Express(text,voiceType,voice);
                //textToSpeak = text;
                if (voice == "Male")
                    this.name = "Microsoft Server Speech Text to Speech Voice (en-US, GuyNeural)";
            }

            public Voice() { }
        }

        //Express class for changing voice types, contains element for prosody
        public class Express
        {
            [XmlAttribute]
            public string type;

            
            [XmlElement(ElementName = "prosody", IsNullable = false)]
            public Prosody prosody;

            public Express(string text,string voiceType,string voice)
            {
                if (voice == "Male")
                    type = "newscast";
                else
                    type = voiceType;
                this.prosody = new Prosody(text,voiceType);
                
            }
            public Express() { }
        }

        //Prosody class to change different aspects of sentences.
        public class Prosody
        {
            [XmlText]
            public string textToSpeak;

            [XmlAttribute]
            public string volume;

            [XmlAttribute]
            public string rate;

            [XmlAttribute]
            public string pitch;

            [XmlAttribute]
            public string contour;

            [XmlAttribute]
            public string range;

            [XmlAttribute]
            public string duration;
            public Prosody(string text,string voiceType)
            {
                rate = "default";
                textToSpeak = text;
            }
            public Prosody() { }
        }
    }

    /// <summary>
    /// Holds the custom speech information we want to pass to our SSML renderer
    /// </summary>
    public class SSMLSettings
    {
        public string text;
        public string voice;
        public SSMLSettings(string textToSpeak, string voice)
        {
            this.text = textToSpeak;
            this.voice = voice;
        }
    }
}