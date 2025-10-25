using ShareLibrary;
using ShareLibrary.Event;
using SubmissionService.Application.Interface;


namespace SubmissionService.Infrastructure
{
    public class RunCodeHandle : IEventHandler<TestCaseFetchEvent>
    {
        //private readonly SubmissionControl _sub;
        //private readonly IEventBus _event;
        private readonly ICompareTestCase _compare;
        private readonly IResultRepository _resultHandle;
        private readonly ISubmissionRepository _submissionHandle;
        public RunCodeHandle(
            //SubmissionControl sub,
            //IEventBus eventBus,
            ICompareTestCase compareTest,
            IResultRepository resultHandle,
            ISubmissionRepository submissionHandle)
        {
            //_sub = sub;
            //_event = eventBus;
            _compare = compareTest;
            _resultHandle = resultHandle;
            _submissionHandle = submissionHandle;
        }



        /// score= (weight* true?false)/ (total (weight))
        public async Task Handle(TestCaseFetchEvent @event)
        {
            double score = 0.0;
            double weightAvg = 0.0;// weight * true/flase
            double totalWeight = 0.0;// total weight 
            foreach (var tc in @event.TestCaseList)
            {
                string sourceCode = @event.SourceCode??"";
                int languageId= @event.LanguageId;
                int assignmentId = @event.AssignmentId;
                int submissionId=@event.SubmissionId;
                // testcase 
                int testcaseId = tc.TestCaseId;
                string stdin = tc.Input??"";
                string exepectedOutput = tc.ExpectedOutput ?? "";
                double weight = tc.Weight;


                // luu bai nop, sau do lay id do len dung de so sanh voi output roi cham diem
                int resultId = await _compare.RunAndAddResult(sourceCode,languageId,stdin,submissionId);

                var resultAfterAdd = await _resultHandle.GetResultById(resultId);
                if (resultAfterAdd == null) { continue; }

                double weightAfterCompare =await _compare.CompareAndUpdateResult
                    (resultAfterAdd.Output??"", exepectedOutput,weight,submissionId,testcaseId,resultId);

                weightAvg += weightAfterCompare;
                totalWeight += weight;
                
            }
            score= (weightAvg/totalWeight)*100;
            if (await _submissionHandle.UpdateScore(score, @event.SubmissionId))
            {
                Console.WriteLine("ok");
            }
            
        }
    }
}
