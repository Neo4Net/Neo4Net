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
namespace Org.Neo4j.Server.rest.security
{
	using Rule = org.junit.Rule;

	using Org.Neo4j.Server.security.enterprise.auth;
	using Org.Neo4j.Server.security.enterprise.auth;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class RESTBuiltInProceduresInteractionIT : BuiltInProceduresInteractionTestBase<RESTSubject>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();

		 public RESTBuiltInProceduresInteractionIT() : base()
		 {
			  ChangePwdErrMsg = "User is required to change their password.";
			  PwdChangeCheckFirst = true;
			  IsEmbedded = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.server.security.enterprise.auth.NeoInteractionLevel<RESTSubject> setUpNeoServer(java.util.Map<String, String> config) throws Throwable
		 public override NeoInteractionLevel<RESTSubject> setUpNeoServer( IDictionary<string, string> config )
		 {
			  return new RESTInteraction( config );
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