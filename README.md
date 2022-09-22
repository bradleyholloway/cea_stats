# PlayCEASharp

This client library fetches and analyzes data from https://app.PlayCEA.com
Configurations support scoping to a specific game and season.

This repository contains the source for the library (/PlayCEASharp) in addition to a command line application for local testing.

## Documentation: https://bradleyholloway.github.io/cea_stats/api/index.html

## Usage

After consuming the package, clients need to have a Resources/configuration.json in their output directory for the library to read.
The included console test app has this setup and you can reference the configuration to see options that can be used:
https://github.com/bradleyholloway/cea_stats/blob/main/PlayCEASharp/CEAClientTest/Resources/configuration.json

### Consumer Example 
This library was initially written for a discord bot, which makes heavy use of the features.
https://github.com/tynidev/tynibot

### Typical Use
The most common entry point will be to initialize the library and load the league of your configuration by calling:
`League league = LeagueManager.League;`
The League object can then be traversed to read all of the data loaded. The high level hierarchy is:
```
* League
    * List<BracketSet>
        * List<Bracket>
            * List<Team>
            * List<BracketRound>
                * List<Match>
                    * List<Game>
                    * HomeTeam
                    * AwayTeam
```
Statistics are tied to Teams, so whenever you have a team object you can find lots of different aggregations of stats pre-computed.
