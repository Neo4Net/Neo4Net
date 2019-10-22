using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;

	using NamedToken = Neo4Net.Internal.Kernel.Api.NamedToken;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsCollectionContaining.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asCollection;

	public class LabelIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllLabels() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllLabels()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int label1Id = transaction.TokenWrite().labelGetOrCreateForName("label1");
			  int label2Id = transaction.TokenWrite().labelGetOrCreateForName("label2");

			  // when
			  IEnumerator<NamedToken> labelIdsBeforeCommit = transaction.TokenRead().labelsGetAllTokens();

			  // then
			  assertThat( asCollection( labelIdsBeforeCommit ), hasItems( new NamedToken( "label1", label1Id ), new NamedToken( "label2", label2Id ) ) );

			  // when
			  Commit();

			  transaction = NewTransaction();
			  IEnumerator<NamedToken> labelIdsAfterCommit = transaction.TokenRead().labelsGetAllTokens();

			  // then
			  assertThat( asCollection( labelIdsAfterCommit ), hasItems( new NamedToken( "label1", label1Id ), new NamedToken( "label2", label2Id ) ) );
			  Commit();
		 }
	}

}