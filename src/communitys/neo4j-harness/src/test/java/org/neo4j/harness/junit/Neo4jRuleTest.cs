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
namespace Neo4Net.Harness.junit
{
	using Test = org.junit.jupiter.api.Test;
	using Statement = org.junit.runners.model.Statement;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runner.Description.createTestDescription;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class Neo4jRuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnHttpsUriWhenConfigured() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReturnHttpsUriWhenConfigured()
		 {
			  URI configuredHttpsUri = URI.create( "https://localhost:7473" );
			  assertEquals( configuredHttpsUri, GetHttpsUriFromNeo4jRule( configuredHttpsUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowWhenHttpsUriNotConfigured() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowWhenHttpsUriNotConfigured()
		 {
			  assertThrows( typeof( System.InvalidOperationException ), () => GetHttpsUriFromNeo4jRule(null) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.net.URI getHttpsUriFromNeo4jRule(java.net.URI configuredHttpsUri) throws Throwable
		 private static URI GetHttpsUriFromNeo4jRule( URI configuredHttpsUri )
		 {
			  ServerControls serverControls = mock( typeof( ServerControls ) );
			  when( serverControls.HttpsURI() ).thenReturn(Optional.ofNullable(configuredHttpsUri));
			  TestServerBuilder serverBuilder = mock( typeof( TestServerBuilder ) );
			  when( serverBuilder.NewServer() ).thenReturn(serverControls);

			  Neo4jRule rule = new Neo4jRule( serverBuilder );

			  AtomicReference<URI> uriRef = new AtomicReference<URI>();
			  Statement statement = rule.apply(new StatementAnonymousInnerClass(rule, uriRef)
			 , createTestDescription( typeof( Neo4jRuleTest ), "test" ));

			  statement.evaluate();
			  return uriRef.get();
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private Neo4Net.Harness.junit.Neo4jRule _rule;
			 private AtomicReference<URI> _uriRef;

			 public StatementAnonymousInnerClass( Neo4Net.Harness.junit.Neo4jRule rule, AtomicReference<URI> uriRef )
			 {
				 this._rule = rule;
				 this._uriRef = uriRef;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void evaluate() throws Throwable
			 public override void evaluate()
			 {
				  _uriRef.set( _rule.httpsURI() );
			 }
		 }
	}

}