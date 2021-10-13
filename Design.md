## Axis.Apollo

### Overview
The Apollo project attempts to provide unified platform for access and modify configuration for applications/systems. It comprises of:
1.  The _persisted_ representation of the _configuration_;
2.  The online service that exposes _apis_ for ccessing or modifying the configuraiton;
3.  _Client library_ through which applications access the configuration;
4.  A _snapshot_ representation of the confgiguration, used for keeping local copies of a snapshot of the configuration.

### Configuration
This section describes the configuration representation as a logical unit.

To cater for a wide variety of scenatios, the configuration is broken into 3 distinct parts:
1. Platform
2. KeyTree (Kree - sounds cool now, may change this name later)
3. ValueTemplate

#### Platform
The platform is used to seprate configurations that belong to disparate entities. The system required to house configuration for more than one 
platform/service/system/etc, thus needing a platform to logically seprate them.

#### KeyTree
Just a step from the domain, Individual values of the configuration are identified by unique keys. The keys by design comprise of successive
components that in turn trace a path along a tree of named-segments. This in turn lends the KeyTree to be represented as a directory structure.


#### ValueTemplate (name may change)
Each tree terminates in a leaf node called a ValueTemplate. A value template represents (resolves to) the value that the key identifies. Values are one of
the following:
1. 64 bit integer (long)
2. 64 bit real (double)
3. 128 bit decimal (decimal)
4. boolean
5. bytes (byte[], or Base64 string)
6. Guid
7. DateTime
8. TimeSpan
9. String
10. list (immutable collection of Values)
11. Struct (mutable map of values)

Things get a bit more interesting because we introduce the concepts of Revisions, and Domains.

##### Revisions
Each value can have an infinite number of revisions. Each published modification to a value's content creates a new revision of that value. This
makes it easier to trace/play back all values ever used for a configuration.

##### Domains
Domains represent a heirarchical (nested) ordering of logical contexts within which a configuration value may exist. 
Any number (n > 0) of contexts may exist. A default context is recognized by the system, and is what ensures that contexts are always n > 0.

Each domain has a name, and possible values, and in addition, holds unto an optional list of revisions of the configuration value. Domains can
be used to simulate things like multi-tenant systems, environments, etc. 


### Design Details

This section takes the concepts introduces in the overview, in no particular order, and expnds on them in more technical detail.

#### ValueTemplate
As stated above, The value template is a construct that ultimately evaluates to a given Type. It recognizes a tree-like heirarchy of domains, each
domain is identified by a type, and instances of that domain are also identified by their domain identifiers/names - essentially forming a key/value pair.
A possible graphical representation will be:

```
.
|-- [tennant:abc]
|   |-- [environment:dev]
|   |-- [environment:beta]
|   |-- [environment:prod]
|
|-- [tennant:xyz]
    |-- [environment:uat]
    |-- [environment:prod]
```

* The structure above assumes there are 2 domains; `tennant` and `environment`. 
* Two instances of the tennant exists: `abc`, and `xyz`
* 4 instances of the environment exist: `dev`, `beta`, `uat`, `prod`.
* The order or occurence of the domain instances is irrelevant to the system.

The idea behind the domains is to have multiple values set for given configuration key, and the final value
resolved depending on the domain of interest.

##### Revisions (ValueRevision)
This is a simple concept to grasp: It represents a list of time-stamp identified versions of a value. By default,
this resolves to the most recent value.


##### Domain
Except for the default domain, each subsequent domain comprises a `domain-type` (tennant, environment, in the previous example), and
a `domain-name`. Both of these have syntaxes validated by the pattern: `[0-9a-zA-Z_-]+(\.[0-9a-zA-Z_-]+)*`, and each domain
contains a ValueRevision.

The default domain has a domain-type of `.`, and a domain-name of `.`, so it's identifier is `.:.` or just `.` for short. It is
the first domain in the heirarchy; subsequent domains are appended as it's descendants.

Value resolution, optionally given a domain path, happens in this order:
1. If no domain path is specified, yield the value from the default domain's ValueRevision's resolution.
2. If a domain path is specified, trace the path along the available domains, yielding the value from resolving the final domain.
3. If the domain path is invalid, either yield the resolved value from the last valid domain, or report an error - depending on settings.

#### KeyTree
Essentially, the `KeyTree` is a graph-representation of a 'typical' json struct, with some restrictions on the name of the object properties.
Property names are restricted by the same set of rules for domains above: `[0-9a-zA-Z_-]+(\.[0-9a-zA-Z_-]+)*`.

The object below:
```
{
   "first-name": "David",
   "last-name": "Denovo",
   "DOB": "06/06/1988",
   "awards": 45,
   "favorite-colors": [
      "Black", "Blue", "White"
   ],
   "socials": {
      "face-book": "denovid_ultra",
      "twitter": "@omni.absent",
      "emails":[
         "david.denovo@ultramail.com",
         "dd_886@gmail.com"
      ]
   }
}
```

Will translate to the following `KeyTree`
```
.
|-- [first-name] <David>
|-- [last-name] <Denovo>
|-- [DOB] <06/06/1988>
|-- [awards] <45>
|-- [favorite-color]
|   |-- [0] <Black>
|   |-- [1] <Blue>
|   |-- [2] <White>
|-- [socials]
    |-- [face-book] <denovid_ultra>
    |-- [twitter] <@omni.absent>
    |-- [emails]
        |-- [0] <david.denovo@ultramail.com>
        |-- [1] <dd_886@gmail.com>

```

Note that this the above is just for demonstration purposes.

In an actual `KeyTree`, each "<..>" value will represent a `ValueTemplate`, and thus extra information will be needed to represent the
domain data.

In the above, navigating to any value can be achieved by separating subsequent node names with a '/'. This will look like the following:
* `first-name` - yielding the value "David"
* `favorite-color/1` - yielding the vlue "Blue"
* `socials/emails/0` - yields the value "david.denovo@ultramail.com"
* `socials/twitter` - yields the value "@omni.absent"

Assuming we had domain 2 domain types, "tennants" and "env", with values same as we had in the previous example, navigating will look like this 
if domain information is used:
* `first-name#abc/beta`
* `socials/emails/0#xyz/uat`


#### Platform
This is a simple, single name whose syntax adheres to the same patten as before: `[0-9a-zA-Z_-]+(\.[0-9a-zA-Z_-]+)*`. Platforms do something
similar to ValueTemplates in that they hold revisions of the entire KeyTree. This is necessary so if there are structural changes to the keytree
are made, any system still depending on the old KeyTree will not automatically stop working.

Platform versions are identified by 2 individually unique properties:
1. A version number (SemVer standard)
2. A time stamp.

Both MUST be present when creating new versions.

### Config Service



