using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Server.security.enterprise.auth
{

	using Org.Neo4j.Graphdb;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using InternalTransaction = Org.Neo4j.Kernel.impl.coreapi.InternalTransaction;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;

	public interface NeoInteractionLevel<S>
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: EnterpriseUserManager getLocalUserManager() throws Exception;
		 EnterpriseUserManager LocalUserManager { get; }

		 GraphDatabaseFacade LocalGraph { get; }

		 FileSystemAbstraction FileSystem();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.kernel.impl.coreapi.InternalTransaction beginLocalTransactionAsUser(S subject, org.neo4j.kernel.api.KernelTransaction.Type txType) throws Throwable;
		 InternalTransaction BeginLocalTransactionAsUser( S subject, KernelTransaction.Type txType );

		 /*
		  * The returned String is empty if the query executed as expected, and contains an error msg otherwise
		  */
		 string ExecuteQuery( S subject, string call, IDictionary<string, object> @params, System.Action<ResourceIterator<IDictionary<string, object>>> resultConsumer );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: S login(String username, String password) throws Exception;
		 S Login( string username, string password );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void logout(S subject) throws Exception;
		 void Logout( S subject );

		 void UpdateAuthToken( S subject, string username, string password );

		 string NameOf( S subject );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void tearDown() throws Throwable;
		 void TearDown();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static String tempPath(String prefix, String suffix) throws java.io.IOException
	//	 {
	//		  Path path = Files.createTempFile(prefix, suffix);
	//		  Files.delete(path);
	//		  return path.toString();
	//	 }

		 void AssertAuthenticated( S subject );

		 void AssertPasswordChangeRequired( S subject );

		 void AssertInitFailed( S subject );

		 void AssertSessionKilled( S subject );

		 string ConnectionProtocol { get; }

		 HostnamePort LookupConnector( string connectorKey );
	}

}