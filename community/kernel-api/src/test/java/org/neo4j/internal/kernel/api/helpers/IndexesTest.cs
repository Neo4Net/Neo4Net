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
namespace Org.Neo4j.@internal.Kernel.Api.helpers
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using Register = Org.Neo4j.Register.Register;
	using Registers = Org.Neo4j.Register.Registers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IndexesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private readonly Org.Neo4j.Register.Register_DoubleLongRegister _register = Registers.newDoubleLongRegister();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTimeOutIfNoIndexes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTimeOutIfNoIndexes()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = schemaWithIndexes();
			  SchemaRead schemaRead = SchemaWithIndexes();

			  // Then no exception
			  Indexes.AwaitResampling( schemaRead, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTimeOutIfNoUpdates() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTimeOutIfNoUpdates()
		 {
			  // Given
			  IndexReference index = mock( typeof( IndexReference ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = schemaWithIndexes(index);
			  SchemaRead schemaRead = SchemaWithIndexes( index );
			  SetUpdates( schemaRead, 0 );

			  // Then no exception
			  Indexes.AwaitResampling( schemaRead, 0 );
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAwaitIndexResampling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAwaitIndexResampling()
		 {
			  // Given
			  IndexReference index = mock( typeof( IndexReference ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = schemaWithIndexes(index);
			  SchemaRead schemaRead = SchemaWithIndexes( index );
			  SetUpdates( schemaRead, 1, 2, 3, 0 );

			  // Then no exception
			  Indexes.AwaitResampling( schemaRead, 60 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAwaitIndexResamplingForHeavyLoad() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAwaitIndexResamplingForHeavyLoad()
		 {
			  // Given
			  IndexReference index = mock( typeof( IndexReference ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = schemaWithIndexes(index);
			  SchemaRead schemaRead = SchemaWithIndexes( index );
			  SetUpdates( schemaRead, 1, 2, 3, 2 ); // <- updates went down but didn't reach the first seen value

			  // Then no exception
			  Indexes.AwaitResampling( schemaRead, 60 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTimeout()
		 {
			  // Given
			  IndexReference index = mock( typeof( IndexReference ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = schemaWithIndexes(index);
			  SchemaRead schemaRead = SchemaWithIndexes( index );
			  SetUpdates( schemaRead, 1, 1, 1 );

			  // Then
			  Exception.expect( typeof( TimeoutException ) );
			  Indexes.AwaitResampling( schemaRead, 1 );
		 }

		 private SchemaRead SchemaWithIndexes( params IndexReference[] indexes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.internal.kernel.api.SchemaRead schemaRead = mock(org.neo4j.internal.kernel.api.SchemaRead.class);
			  SchemaRead schemaRead = mock( typeof( SchemaRead ) );
			  when( schemaRead.IndexesGetAll() ).thenReturn(Iterators.iterator(indexes));
			  return schemaRead;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void setUpdates(org.neo4j.internal.kernel.api.SchemaRead schemaRead, int... updates) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void SetUpdates( SchemaRead schemaRead, params int[] updates )
		 {
			  when( schemaRead.IndexUpdatesAndSize( any( typeof( IndexReference ) ), any( typeof( Org.Neo4j.Register.Register_DoubleLongRegister ) ) ) ).thenAnswer( new AnswerAnonymousInnerClass( this, updates ) );
		 }

		 private class AnswerAnonymousInnerClass : Answer<Org.Neo4j.Register.Register_DoubleLongRegister>
		 {
			 private readonly IndexesTest _outerInstance;

			 private int[] _updates;

			 public AnswerAnonymousInnerClass( IndexesTest outerInstance, int[] updates )
			 {
				 this.outerInstance = outerInstance;
				 this._updates = updates;
			 }

			 private int i;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.register.Register_DoubleLongRegister answer(org.mockito.invocation.InvocationOnMock invocationOnMock) throws Throwable
			 public override Org.Neo4j.Register.Register_DoubleLongRegister answer( InvocationOnMock invocationOnMock )
			 {
				  Org.Neo4j.Register.Register_DoubleLongRegister r = invocationOnMock.getArgument( 1 );
				  r.Write( _updates[i], 0 );
				  i = ( i + 1 ) % _updates.Length;
				  return r;
			 }
		 }
	}

}