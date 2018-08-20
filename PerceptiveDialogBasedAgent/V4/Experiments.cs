using PerceptiveDialogBasedAgent.V2;
using PerceptiveDialogBasedAgent.V4.EventBeam;
using PerceptiveDialogBasedAgent.V4.Events;
using PerceptiveDialogBasedAgent.V4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerceptiveDialogBasedAgent.V4
{
    class Experiments
    {
        public static void Explanation()
        {
            var b = new Body();
            b.Input("say bla bla");
            b.Input("just a saying");
        }

        public static void SimpleRestaurantSearch()
        {
            var a = new Agent();
            a.Input("find expensive restaurant");
        }

        public static void RefinementRestaurantSearch()
        {
            var a = new Agent();
            a.Input("find a restaurant");
            //I know many restaurants
            a.Input("some expensive");
            a.Input("what about pricerange");
        }

        public static void LuxuryRestaurantSearch()
        {
            var b = new Body();
            b.Input("find a luxury restaurant");
            b.Input("some luxury restaurant");
            //what is luxury?
            // b.Input("it means nice and expensive");
            b.Input("it means nice and expensive");
        }

        public static void DialogHandling()
        {
            var b = new Body();
            b.Input("find a restaurant");
            b.Input("luxury one");
            b.Input("nice and expensive");
        }


        public static void MissingAction()
        {
            var b = new Body();
            b.Input("luxury restaurant");
            b.Input("expensive");
            b.Input("find a restaurant");
            b.Input("luxury");
        }

        public static void RealData()
        {
            var expensiveAnswer = "It means nice and expensive";
            Body b = null;

            b = new Body();
            b.Input("I need discover a restaurant");
            b.Input("to find");
            b.Input("luxury");
            b.Input(expensiveAnswer);

            b = new Body();
            b.Input("look up the location of the four seasons restaurant and send me the address");
            b.Input("its a place where restaurant is located");

            b = new Body();
            b.Input("I would like to find the four seasons restaurant");
            b.Input(expensiveAnswer);

            b = new Body();
            b.Input("list some luxury restaurants in houston");
            b.Input("luxury");
            b.Input(expensiveAnswer);

            b = new Body();
            b.Input("Can you please list some high style restaurants");
            b.Input(expensiveAnswer);

            b = new Body();
            b.Input("Hello, I want to find a famous restaurant");
            b.Input(expensiveAnswer);

            b = new Body();
            b.Input("find the address of a restaurant");
            b.Input("luxury one");
            b.Input(expensiveAnswer);
        }

        public static void CollectedDataDebugging()
        {
            var b = new Body();
            b.Input("Buen dia me gustaria me ayudaran a ubicar donde me queda Arby’s en New York");
            b.Input("Good day I would like to help me to locate where I stay Arby's in New York");
            b.Input("where I have the restaurant Urasawa");
            b.Input("where I have the luxury restaurant in new york");
        }

        public static void CollectedDataDebugging2()
        {
            var b = new Body();
            b.Input("I need to find a fine restaurant to take my wife");
            b.Input("One where you can order pasta");
            b.Input("I would like to go to the palm restaurant");
            b.Input("elegant");
        }

        public static void CollectedDataDebugging3()
        {
            var b = new Body();
            b.Input("Please help me find the name of a luxury restaurant");
        }

        public static void CollectedDataDebugging4()
        {
            var b = new Body();
            b.Input("The Five Fields");
            b.Input("Contemporary British cuisine restaurant");
            b.Input("I HAVE TO FIND A LUXURIOUS RESTAURANT");
        }

        public static void PropertyReading()
        {
            var b = new Body();
            b.Input("Find an expensive restaurant");
            b.Input("What is its name ?");
            b.Input("and what about its pricerange ?");
        }

        public static void HardRealData()
        {
            var b = new Body();

            b = new Body();
            b.Input("Please send me the address of an Italian restaurant in New York");
        }

        public static void RichDialogTest()
        {
            var agent = new Agent();
            agent.Input("I would like to find something");
            agent.Input("some nice restaurant");
            agent.Input("luxury");
            agent.Input("bla bla");
            agent.Input("expensive");
            agent.Input("what pricerange");
        }

        public static void AdviceProcessing()
        {
            var a = new Agent();
            a.Input("banana is food");
            a.Input("yes it is");
        }

        public static void DialogWithDistractions()
        {
            var agent = new Agent();
            agent.Input("I would like a restaurant for dinner with my wife");
            //I understand the restaurant. However, what should I do?
            agent.Input("find it");
            //I know many restaurants which one would you like?
            agent.Input("luxury");
            agent.Input("bla bla");
            agent.Input("expensive restaurant");
            agent.Input("what are the prices?");
            agent.Input("pricerange");
        }

        public static void AliasTesting()
        {
            var a = new Agent();
            a.Input("give me expensive restaurant");
            a.Reset();

            a.Input("get me expensive restaurant");
            a.Reset();

            a.Input("i want expensive restaurant");
            a.Reset();

            a.Input("search for expensive restaurant");
            a.Reset();

            a.Input("look up expensive restaurant");
            a.Reset();
        }

        public static void UnknownPropertyLearning()
        {
            var a = new Agent();
            a.Input("give me luxury restaurant");
            // what luxury is ?
            a.Input("it is a property");
            // is it a name of some restaurant ?
            a.Input("no it is pricerange");
        }

        public static void UnknownConceptLearning()
        {
            var a = new Agent();
            a.Input("give me banana");
            // what banana is ?
            a.Input("it is a food");
            // I see. I know pizza is food as well but I'm not able to give you a food.
        }

        public static void QuestionDetection()
        {
            var a = new Agent();
            a.Input("is banana yellow");
        }

        public static void InstanceActivationTests()
        {
            var a = new Agent();
            a.Input("expensive");
            a.Input("find the pricerange");
        }

        public static void ConceptCombinationWithReferenceTest()
        {
            var a = new Agent();
            a.Input("expensive restaurant");
            a.Input("find the restaurant");
            a.Input("what about pricerange");
        }

        public static void SubstitutionRequestHandling()
        {
            var a = new Agent();
            a.Input("find");
            a.Input("expensive restaurant");
        }

        public static void UnknownPhraseHandling()
        {
            var a = new Agent();
            //TODO refinement driven unknown phrase handling - try find features for the refined instance
            a.Input("find high style restaurant for my wife");
            a.Input("yes");
            a.Input("something like a price or pricerange");
            a.Input("expensive");
        }

        public static void UnknownWhatHandling()
        {
            var a = new Agent();
            a.Input("what is name of luxury restaurant");
            a.Input("yes");
            a.Input("high price");
            a.Input("expensive");
        }

        public static void WhatHandling()
        {
            var a = new Agent();
            a.Input("what is name of expensive restaurant");
        }

        public static void UnknownPhraseHandlingWithUnknowns()
        {
            var a = new Agent();
            a.Input("find a high style restaurant");
            a.Input("yes foo");
            a.Input("bar");
            a.Input("expensive");
        }

        public static void UnknownPhraseHandlingWithLotOfUnknowns()
        {
            var a = new Agent();
            a.Input("find high style restaurant");
            a.Input("yes foo");
            a.Input("bar");
            a.Input("bar2");
            a.Input("bar3");
            a.Input("expensive");
        }

        public static void UnknownPhraseNothingHandling()
        {
            var a = new Agent();
            a.Input("find a fancy restaurant");
            a.Input("yes");
            a.Input("pricerange");
            a.Input("nothing like that");
        }

        public static void ItHandling()
        {
            var a = new Agent();
            a.Input("expensive restaurant");
            a.Input("find it");
        }

        public static void BeamBranchTesting()
        {
            var b = new BeamGenerator();
            b.Push(new InputPhraseEvent("x"));
            b.Pop();
            b.Push(new InputPhraseEvent("y"));
            b.PushSelf();
        }

        public static void PhraseRestaurant4_Debugging()
        {
            var a = new Agent();
            a.Input("would like to find a luxury restaurant please");
            a.Input("yes i would");
            a.Input("it means fancy or expensive or gourmet");
            a.Input("expensive");
        }

        public static void PhraseRestaurant4_Debugging2()
        {
            var a = new Agent();
            a.Input("Hi, I would like to find a luxury restaurant please");
            a.Input("Yes, an expensive restaurant");
            a.Input("Luxury means high quality and high price");
            a.Input("expensive");
        }

        public static void NewPropertyHandling()
        {
            var a = new Agent();
            a.Input("Bombay serves food with moderate prices");
            a.Input("yes");
        }

        public static void PromptWithUnknownAnswer()
        {
            var a = new Agent();
            a.Input("find a restaurant");
            a.Input("some luxury");
            a.Input("high class");
            a.Input("high price");
            a.Input("expensive");
        }

        public static void PromptWithUnknownAnswer2()
        {
            var a = new Agent();
            a.Input("find a restaurant");
            a.Input("i would like some luxury");
            a.Input("high price");
            a.Input("expensive");
        }

        public static void DoYouKnowHandling()
        {
            var a = new Agent();
            a.Input("do you know bombay?");
            a.Input("it has moderate pricerange.");
            a.Input("yes");
        }

        public static void PartialDoYouKnowHandlingPositive()
        {
            var a = new Agent();
            a.Input("do you know Vapiano pricerange?");
        }

        public static void PartialDoYouKnowHandlingNegative()
        {
            var a = new Agent();
            a.Input("do you know Bombay pricerange?");
            a.Input("it has moderate pricerange");
            a.Input("yes");
        }

        public static void NewPropertyLearning()
        {
            var a = new Agent();
            a.Input("Bombay is located in New York");
            a.Input("It is property of Bombay");
            a.Input("New York");
            a.Input("Yes");
        }

        internal static void CollectedDataDebugging5()
        {
            var a = new Agent();
            a.Input("What can you tell me about Bombay restaurant?");
            a.Input("Bombay restaurant");
        }

        internal static void CollectedDataDebugging6()
        {
            var a = new Agent();
            a.Input("I would like to know more about the Bombay restaurant.");
            a.Input("Tell me about the prices in the restaurant.");
        }

        internal static void FineTuning()
        {
            var a = new Agent();
            a.Input("restuarant");
            a.Input("restaurant");
            a.Input("sure");
            a.Input("find it");
            a.Input("luxury");
            a.Input("high style");
            a.Input("price");
            a.Input("expensive");
        }

        internal static void RepetitiveQuestions()
        {
            var a = new Agent();
            a.Input("find luxury restaurant");
            a.Input("yes");
            a.Input("high style");
            a.Input("pricerange");
            a.Input("expensive");
        }

        internal static void Debugging()
        {
            var a = new Agent();
            a.Input("I want the name of a restaurant");
            a.Input("a restaurant with a expensive price");
        }

        internal static void PropertyLearningDebugging()
        {
            var a = new Agent();
            a.Input("bombay restaurant has moderate prices?");
            a.Input("yes");
        }

        internal static void PropertyLearningDebugging2()
        {
            var a = new Agent();
            a.Input("do you know bombay prices?");
            a.Input("it has moderate prices");
            a.Input("yes");
        }
    }
}
