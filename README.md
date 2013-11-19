HIVEMIND-CSHARP v.0.1
=================

HiveMind-CSharp is a Mono C# based stand-alone AI implementation for the board game Hive by John Yianni.
This project is a port of the original Java-based version which can be found here: 
https://github.com/cmelchior/hivemind

The reason for porting the library to Mono C# is primarely so it easily can be included in eg. the Unity 
3D game engine.

HiveMind-CSharp currently consists of 3 seperate modules:

- A model of the game, including all rules except for the Pillbug (for now).

- A AI game controller that can pitch AI's against each other and output the results including 
several AI implementations.

Going forward it is the intention that this port will contain the most tested and relevant parts
of the Java library, making it as good as it can be for a Unity3D based game, while the Java 
version will act more like a "playground" for testing new AI ideas and work as a game parser /
analytical tool. 


Current status
-----------------
Alpha:

- All rules + AI game controller from Java version ported.
- Only the most current AI's have been ported.
- The Boardspace.net game parser will remain Java only.


Links
-----------------
Hive: http://www.hivemania.com/

Boardspace.net: http://boardspace.net/english/about_hive.html


License
----------------
    Copyright 2013 Christian Melchior

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

