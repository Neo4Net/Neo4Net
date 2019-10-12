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
namespace Org.Neo4j.Kernel.enterprise.builtinprocs
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using InvalidArgumentsException = Org.Neo4j.Kernel.Api.Exceptions.InvalidArgumentsException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.QueryId.fromExternalString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.enterprise.builtinprocs.QueryId.ofInternalId;

	public class QueryIdTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void printsQueryIds() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PrintsQueryIds()
		 {
			  assertThat( ofInternalId( 12L ).ToString(), equalTo("query-12") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotConstructNegativeQueryIds() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotConstructNegativeQueryIds()
		 {
			  Thrown.expect( typeof( InvalidArgumentsException ) );
			  ofInternalId( -15L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void parsesQueryIds() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ParsesQueryIds()
		 {
			  assertThat( fromExternalString( "query-14" ), equalTo( ofInternalId( 14L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotParseNegativeQueryIds() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotParseNegativeQueryIds()
		 {
			  Thrown.expect( typeof( InvalidArgumentsException ) );
			  fromExternalString( "query--12" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotParseRandomText() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotParseRandomText()
		 {
			  Thrown.expect( typeof( InvalidArgumentsException ) );
			  fromExternalString( "blarglbarf" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotParseTrailingRandomText() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotParseTrailingRandomText()
		 {
			  Thrown.expect( typeof( InvalidArgumentsException ) );
			  fromExternalString( "query-12  " );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotParseEmptyText() throws org.neo4j.kernel.api.exceptions.InvalidArgumentsException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DoesNotParseEmptyText()
		 {
			  Thrown.expect( typeof( InvalidArgumentsException ) );
			  fromExternalString( "" );
		 }
	}

}