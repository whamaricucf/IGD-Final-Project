# IGD-Final-Project
Added a public static event System.Action OnEnemyDied to each AI script

In the Die() function I added OnEnemyDied?.Invoke()

In GameManager script I subscribed to these events in Start()

Also created a new HandleEnemyDeath() method which handles adding coins and the counter for enemies defeated

For Exception Handling I wrapped the save file reading and JSON deserialization in a try-catch block