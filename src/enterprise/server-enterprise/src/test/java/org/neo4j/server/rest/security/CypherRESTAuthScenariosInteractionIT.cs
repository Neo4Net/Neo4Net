using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Server.rest.security
{
	using Rule = org.junit.Rule;

	using Neo4Net.Server.security.enterprise.auth;
	using Neo4Net.Server.security.enterprise.auth;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.SuppressOutput.suppressAll;

	public class CypherRESTAuthScenariosInteractionIT : AuthScenariosInteractionTestBase<RESTSubject>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();

		 public CypherRESTAuthScenariosInteractionIT() : base()
		 {
			  ChangePwdErrMsg = "User is required to change their password.";
			  PwdChangeCheckFirst = true;
			  IsEmbedded = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.Neo4Net.server.security.enterprise.auth.NeoInteractionLevel<RESTSubject> setUpNeoServer(java.util.Map<String,String> config) throws Throwable
		 protected internal override NeoInteractionLevel<RESTSubject> setUpNeoServer( IDictionary<string, string> config )
		 {
			  return new CypherRESTInteraction( config );
		 }

		 protected internal override object ValueOf( object obj )
		 {
			  if ( obj is long? )
			  {
					return ( ( long? ) obj ).Value;
			  }
			  else
			  {
					return obj;
			  }
		 }
	}

}