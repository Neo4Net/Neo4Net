﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.configuration
{
	using Test = org.junit.Test;


	using LoadableConfig = Org.Neo4j.Configuration.LoadableConfig;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

	public class AnnotationBasedConfigurationMigratorTest
	{

		 private static readonly AtomicBoolean _wasCalled = new AtomicBoolean( false );

		 internal class SomeSettings : LoadableConfig
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Migrator private static ConfigurationMigrator migrator = (rawConfiguration, log) ->
			  internal static ConfigurationMigrator Migrator = ( rawConfiguration, log ) =>
			  {
				_wasCalled.set( true );
				return rawConfiguration;
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migratorShouldGetPickedUp()
		 public virtual void MigratorShouldGetPickedUp()
		 {

			  // Given
			  AnnotationBasedConfigurationMigrator migrator = new AnnotationBasedConfigurationMigrator( Collections.singleton( new SomeSettings() ) );

			  // When
			  migrator.Apply( new Dictionary<string, string>(), mock(typeof(Log)) );

			  // Then
			  assertThat( _wasCalled.get(), @is(true) );

		 }

	}

}