﻿using System;

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

	using Org.Neo4j.Graphdb.config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.BOOLEAN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	public class GroupConfigTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideNiceSetMechanism()
		 public virtual void ShouldProvideNiceSetMechanism()
		 {
			  assertThat( Connector( 0 ).Enabled.name(), equalTo("dbms.connector.0.enabled") );
		 }

		 internal static ConnectorExample Connector( int key )
		 {
			  return new ConnectorExample( Convert.ToString( key ) );
		 }

		 [Group("dbms.connector")]
		 internal class ConnectorExample
		 {
			  public readonly Setting<bool> Enabled;
			  public readonly Setting<string> Name;

			  internal readonly GroupSettingSupport Group;

			  internal ConnectorExample( string key )
			  {
					Group = new GroupSettingSupport( typeof( ConnectorExample ), key );
					this.Enabled = Group.scope( setting( "enabled", BOOLEAN, FALSE ) );
					this.Name = Group.scope( setting( "name", STRING, "Bob Dylan" ) );
			  }
		 }
	}

}