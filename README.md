# Zoka Test Suite (under development)
Multiplatform (.NET 6), console-based, extensible player of the tests, mainly designated for integration testing.
## Usage
When you have built-up application you may use it from command line like this:
```
ZokaTestSuite [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  suite     Runs the whole suite of tests playlist from the file.
  playlist  Runs the tests playlist from the file.
```
## Basics
The application reads and parses the passed file structured according to the XML and then plays all the tests.
You may run whole *Test suite* or just single *Test playlist*.
### Test playlist
Single test (which may be composed of many smaller actions), which tests some functionality of your SW. E.g. "test, whether your application can create customer record".
Each test is composed from one or many *Test actions*. This *Test action* is smallest unit, the application may perform, e.g. "send HTTP request to the application, which creates customer record".
#### XML Syntax
The *Test playlist* is structured like this:
```xml
<TestsPlaylist name="playlist_name">
	<SomePlaylistAction some_action_param="some_param_value"  other_action_param="$value_from_context" />
	<OtherPlaylistAction _include="/path/to/action/definition.xml"/>
</TestsPlaylist>
```
Example above shows some of the feature the **Zoka Test Suite** offers:
- Test context - each test is run with some context (*DataStorage*), which may be used to transfer information between actions, tests and playlists.
- It is not necessary to write whole tests definition into single file. Possibility to *_include* definition of actions from other file gives you the possibility to structure you test project as it grows big.
### Test suite
TBD
