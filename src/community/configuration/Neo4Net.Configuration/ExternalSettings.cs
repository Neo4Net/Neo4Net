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
namespace Neo4Net.Configuration
{

	using Neo4Net.GraphDb.config;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using Neo4Net.GraphDb.config;

	/// <summary>
	/// This class holds settings which are used external to the java code. This includes things present in the
	/// configuration which are only read and used by the wrapper scripts. By including them here, we suppress warning
	/// messages about Unknown configuration options, and make it possible to document these options via the normal methods.
	/// 
	/// Be aware that values are still validated.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public class ExternalSettings implements LoadableConfig
	public class ExternalSettings : LoadableConfig
	{
		 [Description("Name of the Windows Service.")]
		 public static readonly Setting<string> WindowsServiceName = DummySetting( "dbms.windows_service_name", "Neo4Net" );
		 [Description("Additional JVM arguments. Argument order can be significant. To use a Java commercial feature, the argument to unlock " + "commercial features must precede the argument to enable the specific feature in the config value string. For example, " + "to use Flight Recorder, `-XX:+UnlockCommercialFeatures` must come before `-XX:+FlightRecorder`.")]
		 public static readonly Setting<string> AdditionalJvm = DummySetting( "dbms.jvm.additional" );

		 [Description("Initial heap size. By default it is calculated based on available system resources.")]
		 public static readonly Setting<string> InitialHeapSize = DummySetting( "dbms.memory.heap.initial_size", "", "a byte size (valid units are `k`, `K`, `m`, `M`, `g`, `G`)" );

		 [Description("Maximum heap size. By default it is calculated based on available system resources.")]
		 public static readonly Setting<string> MaxHeapSize = DummySetting( "dbms.memory.heap.max_size", "", "a byte size (valid units are `k`, `K`, `m`, `M`, `g`, `G`)" );

		 private static DummySetting DummySetting( string name )
		 {
			  return new DummySetting( name, "", "a string" );
		 }

		 private static DummySetting DummySetting( string name, string defVal )
		 {
			  return new DummySetting( name, defVal, "a string" );
		 }

		 private static DummySetting DummySetting( string name, string defVal, string valDesc )
		 {
			  return new DummySetting( name, defVal, valDesc );
		 }

		 internal class DummySetting : BaseSetting<string>
		 {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string DefaultValueConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string ValueDescriptionConflict;

			  internal DummySetting( string name, string defVal, string valueDescription )
			  {
					this.NameConflict = name;
					this.DefaultValueConflict = defVal;
					this.ValueDescriptionConflict = valueDescription;
			  }

			  public override string Name()
			  {
					return NameConflict;
			  }

			  public override void WithScope( System.Func<string, string> scopingRule )
			  {

			  }

			  public override string DefaultValue
			  {
				  get
				  {
						return DefaultValueConflict;
				  }
			  }

			  public override string From( Configuration config )
			  {
					return config.Get( this );
			  }

			  public override string Apply( System.Func<string, string> provider )
			  {
					return provider( NameConflict );
			  }

			  public override IList<Setting<string>> Settings( IDictionary<string, string> @params )
			  {
					return Collections.singletonList( this );
			  }

			  public override string ValueDescription()
			  {
					return ValueDescriptionConflict;
			  }
		 }
	}

}