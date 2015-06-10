db-advance is a tool to deploy changes to SQL Server database.

db-advance works with package which is a simple zip file with contents of following structure:

```
+---0001
|   +---Commit
|   |       001.schema.sql
|   |       002.data_TestTable.sql
|   |       003.sp_GetDataFromTestTable.sql
|   |
|   \---Rollback
|           999.schema.sql
|
+---0002
|   +---Commit
|   |       001.schema.sql
|   |
|   \---Rollback
|           999.schema.sql
|
+---0003
|   +---Commit
|   |       001.schema.sql
|   |
|   \---Rollback
|           999.schema.sql
|
\---0004
    +---Commit
    |       001.schema.sql
    |
    \---Rollback
            999.schema.sql
```

Each folder represents delta with two parts - commit and rollback. Scripts in folder are executed sequentially in alphanumeric order.

Command line to deploy database:

```
DbAdvance.Host.exe -commit "Data Source=(local);User Id=sa; Password=god" DatabaseName builds\build.zip
```

It will apply all deltas in package sequentially

http://lh3.ggpht.com/-9oWpqwXPxYY/UGs1RV_ChnI/AAAAAAAAaso/xZ1V0pM-RCI/Image%2525281%252529_thumb%25255B2%25255D.png?imgmax=800

The same command could be used to upgrade database to new version. db-advance maintains current version of database in extended properties so it always knows which changes are missing.

http://lh5.ggpht.com/-CwyxLl4d720/UGs1ScuMC9I/AAAAAAAAas4/VXyxBSCwvck/Image%2525282%252529_thumb%25255B1%25255D.png?imgmax=800

The changes also can be rolled back with the rollback command:

```
DbAdvance.Host.exe -rollback "Data Source=(local);User Id=sa; Password=god" DatabaseName builds\build.zip
```

Rollbacks are executed in each delta all the way from the Current Version to the Base Version.

http://lh5.ggpht.com/-3dztmkSUJRs/UGs1TUgoNhI/AAAAAAAAatI/J7rEHddy_wM/Image%2525283%252529_thumb%25255B1%25255D.png?imgmax=800

Base Version can be set up using following command:

```
DbAdvance.Host.exe -setbaseversion "Data Source=(local);User Id=sa; Password=god" DatabaseName 58
```


The last delta in development environment is changes very frequently, so the best approach to have latest version of database in development environment with data preserved is to set base version to the latest build version and after each change rollback database to base version using previous build and apply commits from the next delta.

http://lh6.ggpht.com/-vmikjDdl5uQ/UGs1UR8l-gI/AAAAAAAAatY/VDbU88W5zBY/Image%2525284%252529_thumb%25255B3%25255D.png?imgmax=800

That approach fits nicely to continuous delivery procedure and facilitates storing database in source control.