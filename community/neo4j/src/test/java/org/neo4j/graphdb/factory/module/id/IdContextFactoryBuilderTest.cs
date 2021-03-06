﻿/*
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
namespace Org.Neo4j.Graphdb.factory.module.id
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using KernelTransactionsSnapshot = Org.Neo4j.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using IdGenerator = Org.Neo4j.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdGeneratorImpl = Org.Neo4j.Kernel.impl.store.id.IdGeneratorImpl;
	using IdReuseEligibility = Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using CommunityIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using DefaultFileSystemExtension = Org.Neo4j.Test.extension.DefaultFileSystemExtension;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class IdContextFactoryBuilderTest
	internal class IdContextFactoryBuilderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.io.fs.DefaultFileSystemAbstraction fs;
		 private DefaultFileSystemAbstraction _fs;
		 private readonly JobScheduler _jobScheduler = mock( typeof( JobScheduler ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createCommunityBufferedContextByDefault()
		 internal virtual void CreateCommunityBufferedContextByDefault()
		 {
			  IdContextFactory idContextFactory = IdContextFactoryBuilder.Of( _fs, _jobScheduler ).build();
			  DatabaseIdContext idContext = idContextFactory.CreateIdContext( "database" );

			  IdGeneratorFactory idGeneratorFactory = idContext.IdGeneratorFactory;
			  assertThat( idContext.IdController, instanceOf( typeof( BufferedIdController ) ) );
			  assertThat( idGeneratorFactory, instanceOf( typeof( BufferingIdGeneratorFactory ) ) );

			  ( ( BufferingIdGeneratorFactory )idGeneratorFactory ).initialize( () => mock(typeof(KernelTransactionsSnapshot)) );
			  idGeneratorFactory.Open( _testDirectory.file( "a" ), IdType.NODE, () => 0, 100 ).Dispose();
			  idGeneratorFactory.Open( _testDirectory.file( "b" ), IdType.PROPERTY, () => 0, 100 ).Dispose();

			  BufferingIdGeneratorFactory bufferedFactory = ( BufferingIdGeneratorFactory ) idGeneratorFactory;
			  assertThat( bufferedFactory.Get( IdType.NODE ), instanceOf( typeof( IdGeneratorImpl ) ) );
			  assertThat( bufferedFactory.Get( IdType.PROPERTY ), not( instanceOf( typeof( IdGeneratorImpl ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void requireFileSystemWhenIdGeneratorFactoryNotProvided()
		 internal virtual void RequireFileSystemWhenIdGeneratorFactoryNotProvided()
		 {
			  System.NullReferenceException exception = assertThrows( typeof( System.NullReferenceException ), () => IdContextFactoryBuilder.Of(new CommunityIdTypeConfigurationProvider(), _jobScheduler).build() );
			  assertThat( exception.Message, containsString( "File system is required" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createContextWithCustomIdGeneratorFactoryWhenProvided()
		 internal virtual void CreateContextWithCustomIdGeneratorFactoryWhenProvided()
		 {
			  IdGeneratorFactory idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
			  IdContextFactory contextFactory = IdContextFactoryBuilder.Of( _fs, _jobScheduler ).withIdGenerationFactoryProvider( any => idGeneratorFactory ).build();
			  DatabaseIdContext idContext = contextFactory.CreateIdContext( "database" );

			  IdGeneratorFactory bufferedGeneratorFactory = idContext.IdGeneratorFactory;
			  assertThat( idContext.IdController, instanceOf( typeof( BufferedIdController ) ) );
			  assertThat( bufferedGeneratorFactory, instanceOf( typeof( BufferingIdGeneratorFactory ) ) );

			  ( ( BufferingIdGeneratorFactory )bufferedGeneratorFactory ).initialize( () => mock(typeof(KernelTransactionsSnapshot)) );
			  File file = _testDirectory.file( "a" );
			  IdType idType = IdType.NODE;
			  System.Func<long> highIdSupplier = () => 0;
			  int maxId = 100;

			  idGeneratorFactory.Open( file, idType, highIdSupplier, maxId );

			  verify( idGeneratorFactory ).open( file, idType, highIdSupplier, maxId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createContextWithProvidedReusabilityCheck()
		 internal virtual void CreateContextWithProvidedReusabilityCheck()
		 {
			  IdReuseEligibility reuseEligibility = mock( typeof( IdReuseEligibility ) );
			  IdContextFactory contextFactory = IdContextFactoryBuilder.Of( _fs, _jobScheduler ).withIdReuseEligibility( reuseEligibility ).build();
			  DatabaseIdContext idContext = contextFactory.CreateIdContext( "database" );
			  IdGeneratorFactory bufferedGeneratorFactory = idContext.IdGeneratorFactory;

			  assertThat( bufferedGeneratorFactory, instanceOf( typeof( BufferingIdGeneratorFactory ) ) );
			  BufferingIdGeneratorFactory bufferedFactory = ( BufferingIdGeneratorFactory ) bufferedGeneratorFactory;

			  KernelTransactionsSnapshot snapshot = mock( typeof( KernelTransactionsSnapshot ) );
			  when( snapshot.AllClosed() ).thenReturn(true);

			  bufferedFactory.Initialize( () => snapshot );
			  using ( IdGenerator idGenerator = bufferedFactory.Open( _testDirectory.file( "a" ), IdType.PROPERTY, () => 100, 100 ) )
			  {
					idGenerator.FreeId( 15 );

					bufferedFactory.Maintenance();
					verify( reuseEligibility ).isEligible( snapshot );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createContextWithFactoryWrapper()
		 internal virtual void CreateContextWithFactoryWrapper()
		 {
			  System.Func<IdGeneratorFactory, IdGeneratorFactory> factoryWrapper = mock( typeof( System.Func ) );
			  IdGeneratorFactory idGeneratorFactory = mock( typeof( IdGeneratorFactory ) );
			  when( factoryWrapper( any() ) ).thenReturn(idGeneratorFactory);

			  IdContextFactory contextFactory = IdContextFactoryBuilder.Of( _fs, _jobScheduler ).withFactoryWrapper( factoryWrapper ).build();

			  DatabaseIdContext idContext = contextFactory.CreateIdContext( "database" );

			  assertSame( idGeneratorFactory, idContext.IdGeneratorFactory );
		 }
	}

}