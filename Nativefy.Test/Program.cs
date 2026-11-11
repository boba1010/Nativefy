//using Nativefy.Engine.Feed;
//using Nativefy.Engine.Playable;

//var mediaA = CreateMedia("A", "Learn C# Programming");
//var mediaB = CreateMedia("B", "C# Memory Management");
//var mediaC = CreateMedia("C", "GPU Graphics Programming");
//var mediaD = CreateMedia("D", "Game Engine Architecture");

//var candidates = new List<RankedCandidate>
//{
//    new(mediaA, 2.0f),
//    new(mediaB, 1.5f),
//    new(mediaA, 1.0f), // Duplicate
//    new(mediaC, 0.8f),
//    new(mediaD, 0.5f)
//};

//var watchHistory = new List<WatchActivity>
//{
//    new(mediaB, 0.95f)
//};

//Console.WriteLine("========== INPUT CANDIDATES ==========");

//foreach (var candidate in candidates)
//    Console.WriteLine($"{candidate.Score:F2} | {candidate.Media.Title}");

//Console.WriteLine("\n========== WATCH HISTORY ==========");

//foreach (var watch in watchHistory)
//    Console.WriteLine($"{watch.Media.Id} | {watch.Media.Title} | {watch.Completion:P0}");

//var result = FeedFilter.Filter(candidates, watchHistory, maxResults: 3);

//Console.WriteLine("\n========== FILTERED FEED ==========");

//foreach (var candidate in result)
//    Console.WriteLine($"{candidate.Score:F2} | {candidate.Media.Title}");

//Console.WriteLine($"\nTotal results: {result.Count}");

//Console.WriteLine("\n========== TEST ASSERTIONS ==========");

//Assert(result.Count == 3, "Result limit");
//Assert(result[0].Media.Id == "A", "Highest-ranked candidate retained");
//Assert(result[1].Media.Id == "C", "Watched candidate excluded");
//Assert(result[2].Media.Id == "D", "Duplicate candidate excluded");

//Assert(result.All(x => x.Media.Id != "B"), "Watched videos excluded");

//Assert(result.Select(x => x.Media.Id).Distinct().Count() == result.Count, "No duplicate IDs");

//Console.WriteLine("\nAll tests passed!");

//static PlayableMedia CreateMedia(string id, string title)
//{
//    //return new PlayableMedia(new Nativefy.Engine.Search.Video(id, title));
//}

//static void Assert(bool condition, string name)
//{
//    Console.WriteLine($"{(condition ? "[PASS]" : "[FAIL]")} {name}");

//    if (!condition)
//        throw new Exception($"Test failed: {name}");
//}

//// Minimal test adapter for the Search.Video constructor.
//// Replace this with your actual Search.Video wrapper if needed.