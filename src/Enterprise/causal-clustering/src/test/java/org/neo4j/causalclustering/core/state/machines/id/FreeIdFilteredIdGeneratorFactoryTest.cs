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
namespace Neo4Net.causalclustering.core.state.machines.id
{
	using Test = org.junit.Test;


	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class FreeIdFilteredIdGeneratorFactoryTest
	{
		 private readonly IdGeneratorFactory _idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
		 private readonly File _file = mock( typeof( File ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void openFilteredGenerator()
		 public virtual void OpenFilteredGenerator()
		 {
			  FreeIdFilteredIdGeneratorFactory filteredGenerator = CreateFilteredFactory();
			  IdType idType = IdType.NODE;
			  long highId = 1L;
			  long maxId = 10L;
			  System.Func<long> highIdSupplier = () => highId;
			  IdGenerator idGenerator = filteredGenerator.Open( _file, idType, highIdSupplier, maxId );

			  verify( _idGeneratorFactory ).open( eq( _file ), eq( idType ), any( typeof( System.Func<long> ) ), eq( maxId ) );
			  assertThat( idGenerator, instanceOf( typeof( FreeIdFilteredIdGenerator ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void openFilteredGeneratorWithGrabSize()
		 public virtual void OpenFilteredGeneratorWithGrabSize()
		 {
			  FreeIdFilteredIdGeneratorFactory filteredGenerator = CreateFilteredFactory();
			  IdType idType = IdType.NODE;
			  long highId = 1L;
			  long maxId = 10L;
			  int grabSize = 5;
			  System.Func<long> highIdSupplier = () => highId;
			  IdGenerator idGenerator = filteredGenerator.Open( _file, grabSize, idType, highIdSupplier, maxId );

			  verify( _idGeneratorFactory ).open( _file, grabSize, idType, highIdSupplier, maxId );
			  assertThat( idGenerator, instanceOf( typeof( FreeIdFilteredIdGenerator ) ) );
		 }

		 private FreeIdFilteredIdGeneratorFactory CreateFilteredFactory()
		 {
			  return new FreeIdFilteredIdGeneratorFactory( _idGeneratorFactory, () => true );
		 }
	}

}