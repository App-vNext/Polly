# Custom policies

From Polly v7.0 it is possible to [create your own custom policies](https://www.thepollyproject.org/2019/02/13/introducing-custom-polly-policies-and-polly-contrib-custom-policies-part-i/) outside Polly. These custom policies can integrate in to all the existing goodness from Polly: the `Policy.Handle<>()` syntax; `PolicyWrap`; all the execution-dispatch overloads.

For more info see our blog series:

+ [Part I: Introducing custom Polly policies and the Polly.Contrib](https://www.thepollyproject.org/2019/02/13/introducing-custom-polly-policies-and-polly-contrib-custom-policies-part-i/)
+ [Part II: Authoring a non-reactive custom policy](https://www.thepollyproject.org/2019/02/13/authoring-a-proactive-polly-policy-custom-policies-part-ii/) (a policy which acts on all executions)
+ [Part III: Authoring a reactive custom policy](https://www.thepollyproject.org/2019/02/13/authoring-a-reactive-polly-policy-custom-policies-part-iii-2/) (a policy which react to faults).
+ [Part IV: Custom policies for all execution types](https://www.thepollyproject.org/2019/02/13/custom-policies-for-all-execution-types-custom-policies-part-iv/): sync and async, generic and non-generic.

We provide a [starter template for a custom policy](https://github.com/Polly-Contrib/Polly.Contrib.CustomPolicyTemplates) for developing your own custom policy.
