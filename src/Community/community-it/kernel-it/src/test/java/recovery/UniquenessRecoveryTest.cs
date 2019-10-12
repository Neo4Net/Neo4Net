using System;
using System.Collections.Generic;
using System.IO;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Recovery
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ConstraintDefinition = Neo4Net.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.Graphdb.schema.ConstraintType;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Boolean.getBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppress;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class UniquenessRecoveryTest
	public class UniquenessRecoveryTest
	{
		 /// <summary>
		 /// This test can be configured (via system property) to use cypher or the core API to exercise the db. </summary>
		 private static readonly bool _useCypher = getBoolean( Param( "use_cypher" ) );
		 /// <summary>
		 /// This test can be configured (via system property) to run with all different kill signals. </summary>
		 public static readonly bool Exhaustive = getBoolean( Param( "exhaustive" ) );

		 /// <summary>
		 /// these are all the kill signals that causes a JVM to exit </summary>
		 private static readonly int[] _killSignals = new int[] { 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 24, 26, 27, 30, 31 };

		 private static string Param( string name )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return typeof( UniquenessRecoveryTest ).FullName + "." + name;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput muted = suppress(org.neo4j.test.rule.SuppressOutput.System.out);
		 public readonly SuppressOutput Muted = suppress( SuppressOutput.System.out );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();
		 private readonly Configuration _config;

		 private static readonly System.Reflection.FieldInfo _pid;

		 static UniquenessRecoveryTest()
		 {
			  System.Reflection.FieldInfo pid;
			  try
			  {
					pid = Type.GetType( "java.lang.UNIXProcess" ).getDeclaredField( "pid" );
					pid.Accessible = true;
			  }
			  catch ( Exception )
			  {
					pid = null;
			  }
			  _pid = pid;
		 }

		 /// <summary>
		 /// This test uses sub-processes, the code in here is the orchestration of those processes. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpholdConstraintEvenAfterRestart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpholdConstraintEvenAfterRestart()
		 {
			  assumeNotNull( _pid ); // this test can only run on UNIX

			  // given
			  File path = Dir.databaseDir().AbsoluteFile;
			  Console.WriteLine( "in path: " + path );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  ProcessBuilder prototype = new ProcessBuilder( "java", "-ea", "-Xmx1G", "-Djava.awt.headless=true", "-Dforce_create_constraint=" + _config.force_create_constraint, "-D" + Param( "use_cypher" ) + "=" + _useCypher, "-cp", System.getProperty( "java.class.path" ), this.GetType().FullName, path.Path );
			  prototype.environment().put("JAVA_HOME", System.getProperty("java.home"));

			  {
			  // when
					Console.WriteLine( "== first subprocess ==" );
					Process process = prototype.start();
					if ( AwaitMessage( process, "kill me" ) != null )
					{
						 throw new System.InvalidOperationException( "first process failed to execute properly" );
					}
					Kill( _config.kill_signal, process );
					AwaitMessage( process, null );
			  }
			  {
					Console.WriteLine( "== second subprocess ==" );
					Process process = prototype.start();
					int? exitCode = AwaitMessage( process, "kill me" );
					if ( exitCode == null )
					{
						 Kill( _config.kill_signal, process );
						 AwaitMessage( process, null );
					}
					else if ( exitCode != 0 )
					{
						 Console.WriteLine( "! second process did not exit in an expected manner" );
					}
			  }

			  // then
			  GraphDatabaseService db = Graphdb( path );
			  try
			  {
					ShouldHaveUniquenessConstraintForNamePropertyOnPersonLabel( db );
					NodesWithPersonLabelHaveUniqueName( db );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 /// <summary>
		 /// This is the code that the test actually executes to attempt to violate the constraint. </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String... args) throws Exception
		 public static void Main( params string[] args )
		 {
			  Console.WriteLine( "hello world" );
			  File path = new File( args[0] );
			  bool createConstraint = getBoolean( "force_create_constraint" ) || !( new File( path, "neostore" ) ).File;
			  GraphDatabaseService db = Graphdb( path );
			  Console.WriteLine( "database started" );
			  Console.WriteLine( "createConstraint = " + createConstraint );
			  if ( createConstraint )
			  {
					try
					{
						 Console.WriteLine( "> creating constraint" );
						 createConstraint( db );
						 Console.WriteLine( "< created constraint" );
					}
					catch ( Exception e )
					{
						 Console.WriteLine( "!! failed to create constraint" );
						 e.printStackTrace( System.out );
						 if ( e is ConstraintViolationException )
						 {
							  Console.WriteLine( "... that is ok, since it means that constraint already exists ..." );
						 }
						 else
						 {
							  Environment.Exit( 1 );
						 }
					}
			  }
			  try
			  {
					Console.WriteLine( "> adding node" );
					AddNode( db );
					Console.WriteLine( "< added node" );
			  }
			  catch ( ConstraintViolationException e )
			  {
					Console.WriteLine( "!! failed to add node" );
					e.printStackTrace( System.out );
					Console.WriteLine( "... this is probably what we want :) -- [but let's let the parent process verify]" );
					Db.shutdown();
					Environment.Exit( 0 );
			  }
			  catch ( Exception e )
			  {
					Console.WriteLine( "!! failed to add node" );
					e.printStackTrace( System.out );
					Environment.Exit( 2 );
			  }

			  FlushPageCache( db );
			  Console.WriteLine( "kill me" );
			  Await();
		 }

		 // ASSERTIONS

		 private static void ShouldHaveUniquenessConstraintForNamePropertyOnPersonLabel( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					ConstraintDefinition constraint = Iterables.single( Db.schema().Constraints );
					assertEquals( ConstraintType.UNIQUENESS, constraint.ConstraintType );
					assertEquals( "Person", constraint.Label.name() );
					assertEquals( "name", Iterables.single( constraint.PropertyKeys ) );

					tx.Success();
			  }
		 }

		 private static void NodesWithPersonLabelHaveUniqueName( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					using ( ResourceIterator<Node> person = Db.findNodes( label( "Person" ) ) )
					{
						 ISet<object> names = new HashSet<object>();
						 while ( person.MoveNext() )
						 {
							  object name = person.Current.getProperty( "name", null );
							  if ( name != null )
							  {
									assertTrue( "non-unique name: " + name, names.Add( name ) );
							  }
						 }
					}

					tx.Success();
			  }
		 }

		 // UTILITIES used for execution

		 private static void CreateConstraint( GraphDatabaseService db )
		 {
			  if ( _useCypher )
			  {
					Db.execute( "create constraint on (p:Person) assert p.name is unique" );
			  }
			  else
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.schema().constraintFor(label("Person")).assertPropertyIsUnique("name").create();

						 tx.Success();
					}
			  }
		 }

		 private static void AddNode( GraphDatabaseService db )
		 {
			  if ( _useCypher )
			  {
					Result result = Db.execute( "create (:Person {name: 'Sneaky Steve'})" );
					Console.WriteLine( result.ResultAsString() );
			  }
			  else
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Db.createNode( label( "Person" ) ).setProperty( "name", "Sneaky Steve" );

						 tx.Success();
					}
			  }
		 }

		 private static GraphDatabaseService Graphdb( File path )
		 {
			  return ( new TestGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(path).newGraphDatabase();
		 }

		 private static void FlushPageCache( GraphDatabaseService db )
		 {
			  try
			  {
					( ( GraphDatabaseAPI ) db ).DependencyResolver.resolveDependency( typeof( PageCache ) ).flushAndForce();
			  }
			  catch ( IOException e )
			  {
					Console.WriteLine( "!! failed to force the page cache" );
					e.printStackTrace( System.out );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void await() throws java.io.IOException
		 internal static void Await()
		 {
			  Console.Read();
		 }

		 // PARAMETERIZATION

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Object[]> configurations()
		 public static IList<object[]> Configurations()
		 {
			  List<object[]> configurations = new List<object[]>();
			  if ( Exhaustive )
			  {
					foreach ( int killSignal in _killSignals )
					{
						 configurations.Add( ( new Configuration() ).ForceCreateConstraint(true).kill_signal(killSignal).build() );
						 configurations.Add( ( new Configuration() ).ForceCreateConstraint(false).kill_signal(killSignal).build() );
					}
			  }
			  else
			  {
					configurations.Add( ( new Configuration() ).Build() );
			  }
			  return configurations;
		 }

		 public class Configuration
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ForceCreateConstraintConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int KillSignalConflict = 9;

			  public virtual Configuration ForceCreateConstraint( bool forceCreateConstraint )
			  {
					this.ForceCreateConstraintConflict = forceCreateConstraint;
					return this;
			  }

			  public virtual object[] Build()
			  {
					return new object[]{ this };
			  }

			  public virtual Configuration KillSignal( int killSignal )
			  {
					this.KillSignalConflict = killSignal;
					return this;
			  }

			  public override string ToString()
			  {
					return "Configuration{" +
							 "use_cypher=" + _useCypher +
							 ", force_create_constraint=" + ForceCreateConstraintConflict +
							 ", kill_signal=" + KillSignalConflict +
							 '}';
			  }
		 }

		 public UniquenessRecoveryTest( Configuration config )
		 {
			  this._config = config;
		 }

		 // UTILITIES for process management

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String pidOf(Process process) throws Exception
		 private static string PidOf( Process process )
		 {
			  return _pid.get( process ).ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void kill(int signal, Process process) throws Exception
		 private static void Kill( int signal, Process process )
		 {
			  int exitCode = ( new ProcessBuilder( "kill", "-" + signal, PidOf( process ) ) ).start().waitFor();
			  if ( exitCode != 0 )
			  {
					throw new System.InvalidOperationException( "<kill -" + signal + "> failed, exit code: " + exitCode );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<int> awaitMessage(Process process, String message) throws java.io.IOException, InterruptedException
		 private int? AwaitMessage( Process process, string message )
		 {
			  StreamReader @out = new StreamReader( process.InputStream );
			  for ( string line; ( line = @out.ReadLine() ) != null; )
			  {
					Console.WriteLine( line );
					if ( !string.ReferenceEquals( message, null ) && line.contains( message ) )
					{
						 return null;
					}
			  }
			  int exitCode = process.waitFor();
			  StreamReader err = new StreamReader( process.InputStream );
			  for ( string line; ( line = @out.ReadLine() ) != null; )
			  {
					Console.WriteLine( line );
			  }
			  for ( string line; ( line = err.ReadLine() ) != null; )
			  {
					Console.Error.WriteLine( line );
			  }
			  Console.WriteLine( "process exited with exit code: " + exitCode );
			  return exitCode;
		 }
	}

}