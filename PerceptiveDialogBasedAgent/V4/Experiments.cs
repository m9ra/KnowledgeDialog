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
            var body = new Body();
            body.Input("find an expensive restaurant");
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
            a.Input("banana is yellow");
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
            //TODO refinement driver unknown phrase handling - try find features for the refined instance
            a.Input("find high style restaurant for my wife");
            a.Input("yes");
            a.Input("pricerange");
            a.Input("expensive");
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
    }
}
