#load "Scripts/Module.csx" // Contains the Addition method
#load "Scripts/Message.csx" // Contains a message in MessageBox.Show

MessageBox.Show("Hello from Test.csx");

var result1 = Sum(2,2);
MessageBox.Show($"Result of the 1st addition: {result1}");

var result2 = Sum(4, 4);
MessageBox.Show($"Result of the 2nd addition: {result2}");

