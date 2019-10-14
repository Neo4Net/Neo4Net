using System;
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
namespace Neo4Net.Ext.Udc
{

	using Description = Neo4Net.Configuration.Description;
	using Internal = Neo4Net.Configuration.Internal;
	using LoadableConfig = Neo4Net.Configuration.LoadableConfig;
	using Neo4Net.Graphdb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.ANY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.FALSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.HOSTNAME_PORT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.INTEGER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.TRUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.buildSetting;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.illegalValueMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.matches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.setting;

	[Description("Usage Data Collector configuration settings")]
	public class UdcSettings : LoadableConfig
	{
		 /// <summary>
		 /// Configuration key for enabling the UDC extension. </summary>
		 [Description("Enable the UDC extension.")]
		 public static readonly Setting<bool> UdcEnabled = setting( "dbms.udc.enabled", Enabled.UnlessExplicitlyDisabled, Enabled.AS_DEFAULT_VALUE );

		 /// <summary>
		 /// Configuration key for the first delay, expressed in milliseconds. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<int> first_delay = buildSetting("unsupported.dbms.udc.first_delay", INTEGER, System.Convert.ToString(10 * 1000 * 60)).constraint(min(1)).build();
		 public static readonly Setting<int> FirstDelay = buildSetting( "unsupported.dbms.udc.first_delay", INTEGER, Convert.ToString( 10 * 1000 * 60 ) ).constraint( min( 1 ) ).build();

		 /// <summary>
		 /// Configuration key for the interval for regular updates, expressed in milliseconds. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<int> interval = buildSetting("unsupported.dbms.udc.interval", INTEGER, System.Convert.ToString(1000 * 60 * 60 * 24)).constraint(min(1)).build();
		 public static readonly Setting<int> Interval = buildSetting( "unsupported.dbms.udc.interval", INTEGER, Convert.ToString( 1000 * 60 * 60 * 24 ) ).constraint( min( 1 ) ).build();

		 /// <summary>
		 /// The host address to which UDC updates will be sent. Should be of the form hostname[:port]. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<org.neo4j.helpers.HostnamePort> udc_host = setting("unsupported.dbms.udc.host", HOSTNAME_PORT, "udc.neo4j.org");
		 public static readonly Setting<HostnamePort> UdcHost = setting( "unsupported.dbms.udc.host", HOSTNAME_PORT, "udc.neo4j.org" );

		 /// <summary>
		 /// Configuration key for overriding the source parameter in UDC </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<String> udc_source = buildSetting("unsupported.dbms.udc.source", STRING, "maven").constraint(illegalValueMessage("Must be a valid source", matches(ANY))).build();
		 public static readonly Setting<string> UdcSource = buildSetting( "unsupported.dbms.udc.source", STRING, "maven" ).constraint( illegalValueMessage( "Must be a valid source", matches( ANY ) ) ).build();

		 /// <summary>
		 /// Unique registration id </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Internal public static final org.neo4j.graphdb.config.Setting<String> udc_registration_key = buildSetting("unsupported.dbms.udc.reg", STRING, "unreg").constraint(illegalValueMessage("Must be a valid registration id", matches(ANY))).build();
		 public static readonly Setting<string> UdcRegistrationKey = buildSetting( "unsupported.dbms.udc.reg", STRING, "unreg" ).constraint( illegalValueMessage( "Must be a valid registration id", matches( ANY ) ) ).build();

		 private sealed class Enabled : Function<string, bool>
		 {
			  /// <summary>
			  /// Only explicitly configuring this as 'false' disables UDC, all other values leaves UDC enabled. </summary>
			  public static readonly Enabled UnlessExplicitlyDisabled = new Enabled( "UnlessExplicitlyDisabled", InnerEnum.UnlessExplicitlyDisabled );

			  private static readonly IList<Enabled> valueList = new List<Enabled>();

			  static Enabled()
			  {
				  valueList.Add( UnlessExplicitlyDisabled );
			  }

			  public enum InnerEnum
			  {
				  UnlessExplicitlyDisabled
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Enabled( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  /// <summary>
			  /// Explicitly allocate a String here so that we know it is unique and can do identity equality comparisons on it
			  /// to detect that the default value has been used.
			  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("RedundantStringConstructorCall") static final String AS_DEFAULT_VALUE = new String(TRUE);
			  internal Static readonly;

			  public bool? Apply( string from )
			  {
					// Perform identity equality here to differentiate between the default value (which is explicitly allocated
					// as a new instance, and is thus known to be unique), and explicitly being configured as "true".
					//noinspection StringEquality
					if ( string.ReferenceEquals( from, AS_DEFAULT_VALUE ) ) // yes, this should really be ==
					{ // the default value, as opposed to explicitly configured to "true"
						 // Should result in UDC being enabled, unless one of the other ways to configure explicitly disables it
						 string enabled = System.getProperty( udc_enabled.Name() );
						 if ( FALSE.equalsIgnoreCase( enabled ) )
						 { // the 'enabled' system property tries to disable UDC
							  string disabled = System.getProperty( UdcDisabled() );
							  if ( string.ReferenceEquals( disabled, null ) || disabled.Equals( TRUE, StringComparison.OrdinalIgnoreCase ) )
							  { // the 'disabled' system property does nothing to enable UDC
									return false;
							  }
						 }
						 else if ( TRUE.equalsIgnoreCase( System.getProperty( UdcDisabled() ) ) )
						 { // the 'disabled' system property tries to disable UDC
							  return !string.ReferenceEquals( enabled, null ); // only disable if 'enabled' was not defined
						 }
						 return true;
					}
					else if ( FALSE.equalsIgnoreCase( from ) )
					{ // the setting tries to disable UDC
						 // if any other way of configuring UDC enables it, trust that instead.
						 string enabled = System.getProperty( udc_enabled.Name() );
						 string disabled = System.getProperty( UdcDisabled() );
						 if ( string.ReferenceEquals( enabled, null ) || enabled.Equals( FALSE, StringComparison.OrdinalIgnoreCase ) )
						 { // the 'enabled' system property does nothing to enable UDC
							  if ( string.ReferenceEquals( disabled, null ) || disabled.Equals( TRUE, StringComparison.OrdinalIgnoreCase ) )
							  { // the 'disabled' system property does nothing to enable UDC
									return false;
							  }
						 }
						 return true;
					}
					else
					{ // the setting enabled UDC
						 return true;
					}
			  }

			  public override string ToString()
			  {
					return "a boolean";
			  }

			  internal static string UdcDisabled()
			  {
					return udc_enabled.Name().Replace("enabled", "disable");
			  }

			 public static IList<Enabled> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public static Enabled valueOf( string name )
			 {
				 foreach ( Enabled enumInstance in Enabled.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}