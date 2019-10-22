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
	using AfterClass = org.junit.AfterClass;
	using Before = org.junit.Before;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;

	/// <summary>
	/// KernelAPIWriteTestBase is the basis of write tests targeting the Kernel API.
	/// 
	/// Just as with KernelAPIReadTestBase, write tests cannot provide all the functionality needed to construct the
	/// test kernel, and also do not know how to assert the effects of the writes. These things are abstracted behind the
	/// KernelAPIWriteTestSupport interface, which needs to be implemented to test Kernel write implementations.
	/// 
	/// Since write tests modify the graph, the test graph is recreated on every test run.
	/// </summary>
	/// @param <WriteSupport> The test support for the current test. </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public abstract class KernelAPIWriteTestBase<WriteSupport extends KernelAPIWriteTestSupport>
	public abstract class KernelAPIWriteTestBase<WriteSupport> where WriteSupport : KernelAPIWriteTestSupport
	{
		 protected internal static readonly TemporaryFolder Folder = new TemporaryFolder();
		 protected internal static KernelAPIWriteTestSupport TestSupport;
		 protected internal static IGraphDatabaseService GraphDb;

		 /// <summary>
		 /// Creates a new instance of WriteSupport, which will be used to execute the concrete test
		 /// </summary>
		 public abstract WriteSupport NewTestSupport();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetupGraph()
		 {
			  if ( TestSupport == null )
			  {
					Folder.create();
					TestSupport = NewTestSupport();
					TestSupport.setup( Folder.Root );
					GraphDb = TestSupport.graphBackdoor();
			  }
			  TestSupport.clearGraph();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected Transaction beginTransaction() throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException
		 protected internal virtual Transaction BeginTransaction()
		 {
			  Kernel kernel = TestSupport.kernelToTest();
			  return kernel.BeginTransaction( Transaction_Type.Implicit, LoginContext.AUTH_DISABLED );
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