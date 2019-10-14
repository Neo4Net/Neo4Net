/// <summary>
/// See https://raw.githubusercontent.com/neo4j/neo4j/3.3/enterprise/server-enterprise/src/test/java/org/neo4j/server/enterprise/OpenEnterpriseEntryPointTest.java
/// </summary>

namespace Neo4Net.Server.enterprise
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.commandline.Util.neo4jVersion;

	public class EnterpriseEntryPointTest
	{
		 private PrintStream _realSystemOut;
		 private PrintStream _fakeSystemOut;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _realSystemOut = System.out;
			  _fakeSystemOut = mock( typeof( PrintStream ) );
			  System.Out = _fakeSystemOut;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  System.Out = _realSystemOut;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mainPrintsVersion()
		 public virtual void MainPrintsVersion()
		 {
			  // when
			  EnterpriseEntryPoint.Main( new string[]{ "--version" } );

			  // then
			  verify( _fakeSystemOut ).println( "neo4j " + neo4jVersion() );
			  verifyNoMoreInteractions( _fakeSystemOut );
		 }
	}

}