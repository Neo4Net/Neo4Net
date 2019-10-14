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
namespace Neo4Net.Kernel.impl.store.id
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using IdCapacityExceededException = Neo4Net.Kernel.impl.store.id.validation.IdCapacityExceededException;
	using NegativeIdException = Neo4Net.Kernel.impl.store.id.validation.NegativeIdException;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IdGeneratorImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsr = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fsr = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

		 private readonly File _file = new File( "ids" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptMinusOne()
		 public virtual void ShouldNotAcceptMinusOne()
		 {
			  // GIVEN
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, false, IdType.Node, () => 0L );

			  ExpectedException.expect( typeof( NegativeIdException ) );

			  // WHEN
			  idGenerator.HighId = -1;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenNextIdIsTooHigh()
		 public virtual void ThrowsWhenNextIdIsTooHigh()
		 {
			  long maxId = 10;
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 1, maxId, false, IdType.Node, () => 0L );

			  for ( long i = 0; i <= maxId; i++ )
			  {
					idGenerator.NextId();
			  }

			  ExpectedException.expect( typeof( IdCapacityExceededException ) );
			  ExpectedException.expectMessage( "Maximum id limit for NODE has been reached. Generated id 11 is out of " + "permitted range [0, 10]." );
			  idGenerator.NextId();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void throwsWhenGivenHighIdIsTooHigh()
		 public virtual void ThrowsWhenGivenHighIdIsTooHigh()
		 {
			  long maxId = 10;
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 1, maxId, false, IdType.RelationshipTypeToken, () => 0L );

			  ExpectedException.expect( typeof( IdCapacityExceededException ) );
			  ExpectedException.expectMessage( "Maximum id limit for RELATIONSHIP_TYPE_TOKEN has been reached. Generated id 11 is out of permitted range [0, 10]." );
			  idGenerator.HighId = maxId + 1;
		 }

		 /// <summary>
		 /// It should be fine to set high id to <seealso cref="IdGeneratorImpl.INTEGER_MINUS_ONE"/>.
		 /// It will just be never returned from <seealso cref="IdGeneratorImpl.nextId()"/>.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void highIdCouldBeSetToReservedId()
		 public virtual void HighIdCouldBeSetToReservedId()
		 {
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 0, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 1, long.MaxValue, false, IdType.Node, () => 0L );

			  idGenerator.HighId = IdGeneratorImpl.INTEGER_MINUS_ONE;

			  assertEquals( IdGeneratorImpl.INTEGER_MINUS_ONE + 1, idGenerator.NextId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctDefragCountWhenHaveIdsInFile()
		 public virtual void CorrectDefragCountWhenHaveIdsInFile()
		 {
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 100, false );
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, true, IdType.Node, () => 100L );

			  idGenerator.FreeId( 5 );
			  idGenerator.Dispose();

			  IdGenerator reloadedIdGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, true, IdType.Node, () => 100L );
			  assertEquals( 1, reloadedIdGenerator.DefragCount );
			  assertEquals( 5, reloadedIdGenerator.NextId() );
			  assertEquals( 0, reloadedIdGenerator.DefragCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadHighIdUsingStaticMethod() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadHighIdUsingStaticMethod()
		 {
			  // GIVEN
			  long highId = 12345L;
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, highId, false );

			  // WHEN
			  long readHighId = IdGeneratorImpl.ReadHighId( Fsr.get(), _file );

			  // THEN
			  assertEquals( highId, readHighId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadDefragCountUsingStaticMethod() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadDefragCountUsingStaticMethod()
		 {
			  EphemeralFileSystemAbstraction fs = Fsr.get();
			  IdGeneratorImpl.CreateGenerator( fs, _file, 0, false );
			  IdGeneratorImpl idGenerator = new IdGeneratorImpl( fs, _file, 1, 10000, false, IdType.Node, () => 0L );
			  idGenerator.NextId();
			  long a = idGenerator.NextId();
			  idGenerator.NextId();
			  long b = idGenerator.NextId();
			  idGenerator.NextId();
			  idGenerator.FreeId( a );
			  idGenerator.FreeId( b );
			  long expectedDefragCount = idGenerator.DefragCount;
			  idGenerator.Dispose();

			  long actualDefragCount = IdGeneratorImpl.ReadDefragCount( fs, _file );
			  assertEquals( 2, expectedDefragCount );
			  assertEquals( expectedDefragCount, actualDefragCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReadWrittenGenerator()
		 public virtual void ShouldBeAbleToReadWrittenGenerator()
		 {
			  // Given
			  IdGeneratorImpl.CreateGenerator( Fsr.get(), _file, 42, false );
			  IdGeneratorImpl idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, false, IdType.Node, () => 42L );

			  idGenerator.Dispose();

			  // When
			  idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, false, IdType.Node, () => 0L );

			  // Then
			  assertThat( idGenerator.HighId, equalTo( 42L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorShouldCallHighIdSupplierOnNonExistingIdFile()
		 public virtual void ConstructorShouldCallHighIdSupplierOnNonExistingIdFile()
		 {
			  // Given
			  // An empty file (default, nothing to do)
			  // and a mock supplier to test against
			  System.Func<long> highId = mock( typeof( System.Func<long> ) );
			  when( highId() ).thenReturn(0L); // necessary, otherwise it runs into NPE in the constructor below

			  // When
			  // The id generator is started
			  IdGeneratorImpl idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, false, IdType.Node, highId );

			  // Then
			  // The highId supplier must have been called to get the high id
			  verify( highId ).AsLong;

			  idGenerator.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorShouldNotCallHighIdSupplierOnCleanIdFile()
		 public virtual void ConstructorShouldNotCallHighIdSupplierOnCleanIdFile()
		 {
			  // Given
			  // A non empty, clean id file
			  IdContainer.CreateEmptyIdFile( Fsr.get(), _file, 42, true );
			  // and a mock supplier to test against
			  System.Func<long> highId = mock( typeof( System.Func<long> ) );

			  // When
			  // An IdGenerator is created over the previous properly closed file
			  IdGenerator idGenerator = new IdGeneratorImpl( Fsr.get(), _file, 100, 100, false, IdType.Node, highId );
			  idGenerator.Dispose();

			  // Then
			  // The supplier must have remained untouched
			  verifyZeroInteractions( highId );
		 }
	}

}