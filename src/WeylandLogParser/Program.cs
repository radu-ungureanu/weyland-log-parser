using WeylandLogParser;

var filepath = "build.log";
var lines = File.ReadAllLines(filepath);
new ErrorLogging(lines).Log();
