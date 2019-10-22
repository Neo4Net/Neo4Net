/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Internal.Kernel.Api
{
	using After = org.junit.After;
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;

	/// <summary>
	/// KernelAPIReadTestBase is the basis of read tests targeting the Kernel API.
	/// 
	/// As tests are packaged together with the API, they cannot provide all the functionality needed to construct the
	/// test graph, or provide the concrete Kernel to test. These things are abstracted behind the
	/// KernelAPIReadTestSupport interface, which needs to be implemented to test reading Kernel implementations.
	/// 
	/// As read tests do not modify the graph, the test graph is created lazily on the first test run.
	/// </summary>
	/// @param <ReadSupport> The test support for the current test. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public abstract class KernelAPIReadTestBase<ReadSupport extends KernelAPIReadTestSupport>
	public abstract class KernelAPIReadTestBase<ReadSupport> where ReadSupport : KernelAPIReadTestSupport
	{
		private bool InstanceFieldsInitialized = false;

		public KernelAPIReadTestBase()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			CursorsClosedPostCondition = new CursorsClosedPostCondition( () => Cursors );
		}

		 protected internal static readonly TemporaryFolder Folder = new TemporaryFolder();
		 protected internal static KernelAPIReadTestSupport TestSupport;
		 protected internal Transaction Tx;
		 protected internal Read Read;
		 protected internal ExplicitIndexRead IndexRead;
		 protected internal SchemaRead SchemaRead;
		 protected internal Token Token;
		 protected internal ManagedTestCursors Cursors;

		 /// <summary>
		 /// Creates a new instance of KernelAPIReadTestSupport, which will be used to execute the concrete test
		 /// </summary>
		 public abstract ReadSupport NewTestSupport();

		 /// <summary>
		 /// Create the graph which all test in the class will be executed against. The graph is only built once,
		 /// regardless of the number of tests.
		 /// </summary>
		 /// <param name="graphDb"> a graph API which should be used to build the test graph </param>
		 public abstract void CreateTestGraph( IGraphDatabaseService graphDb );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph() throws java.io.IOException, org.Neo4Net.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupGraph()
		 {
			  if ( TestSupport == null )
			  {
					Folder.create();
					TestSupport = NewTestSupport();
					TestSupport.setup( Folder.Root, this.createTestGraph );
			  }
			  Kernel kernel = TestSupport.kernelToTest();
			  Tx = kernel.BeginTransaction( Transaction_Type.Implicit, LoginContext.AUTH_DISABLED );
			  Token = Tx.token();
			  Read = Tx.dataRead();
			  IndexRead = Tx.indexRead();
			  SchemaRead = Tx.schemaRead();
			  Cursors = new ManagedTestCursors( Tx.cursors() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Transaction beginTransaction() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Transaction BeginTransaction()
		 {
			  Kernel kernel = TestSupport.kernelToTest();
			  return kernel.BeginTransaction( Transaction_Type.Implicit, LoginContext.AUTH_DISABLED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public CursorsClosedPostCondition cursorsClosedPostCondition = new CursorsClosedPostCondition(() -> cursors);
		 public CursorsClosedPostCondition CursorsClosedPostCondition;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseTransaction()
		 {
			  Tx.success();
			  Tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDown()
		 public static void TearDown()
		 {
			  if ( TestSupport != null )
			  {
					TestSupport.tearDown();
					Folder.delete();
					TestSupport = null;
			  }
		 }
	}

}