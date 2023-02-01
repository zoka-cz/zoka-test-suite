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
The *Test playlist* file is structured like this:
```xml
<TestPlaylist _name="playlist_name">
	<SomePlaylistAction some_action_param="some_param_value"  other_action_param="$value_from_context" />
	<OtherPlaylistAction _include="/path/to/action/definition.xml"/>
</TestPlaylist>
```
Example above shows some of the feature the **Zoka Test Suite** offers:
- Test context - each test is run with some context (*DataStorage*), which may be used to transfer information between actions, tests and playlists.
- It is not necessary to write whole tests definition into single file. Possibility to *_include* definition of actions from other file gives you the possibility to structure you test project as it grows big.

For full syntax see the Syntax chapter.
### Test suite
Test suite is collection of *Test playlists*, which are run in the given sequence. For more clarity, you may express the meaning of the *Test suite* by natural language like:
- Test, whether the application can create customer record
- Test, whether the application can delete customer record
- Test, whether the order for customer may be created
#### XML Syntax
The *Test suite* file is structured like this:
```xml
<TestSuite _name="testsuite_name">
	<TestPlaylist _name="some playlist name" _include="/path/to/testplaylist/definition.xml" />
	<TestPlaylist _name="other playlist name" _include="/path/to/other/testplaylist/definition.xml" />
	<TestPlaylist _name="another playlist name">
		<SomePlaylistAction some_action_param="some_param_value"  other_action_param="$value_from_context" />
		...
	</TestPlaylist>
</TestSuite>
```
You may see, that definition of the TestPlaylist may be done inline or again included from another file for better readibility.
For full syntax, see the Syntax chapter.
