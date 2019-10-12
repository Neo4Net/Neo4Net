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
namespace Org.Neo4j.Kernel.impl.transaction.log.pruning
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.parseLongWithUnit;

	public class ThresholdConfigParser
	{
		 public sealed class ThresholdConfigValue
		 {
			  internal static readonly ThresholdConfigValue NoPruning = new ThresholdConfigValue( "false", -1 );
			  internal static readonly ThresholdConfigValue KeepLastFile = new ThresholdConfigValue( "entries", 1 );

			  public readonly string Type;
			  public readonly long Value;

			  internal ThresholdConfigValue( string type, long value )
			  {
					this.Type = type;
					this.Value = value;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					ThresholdConfigValue that = ( ThresholdConfigValue ) o;
					return Value == that.Value && Objects.Equals( Type, that.Type );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( Type, Value );
			  }
		 }

		 private ThresholdConfigParser()
		 {
		 }

		 public static ThresholdConfigValue Parse( string configValue )
		 {
			  string[] tokens = configValue.Split( " ", true );
			  if ( tokens.Length == 0 )
			  {
					throw new System.ArgumentException( "Invalid log pruning configuration value '" + configValue + "'" );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String boolOrNumber = tokens[0];
			  string boolOrNumber = tokens[0];

			  if ( tokens.Length == 1 )
			  {
					switch ( boolOrNumber )
					{
					case "keep_all":
					case "true":
						 return ThresholdConfigValue.NoPruning;
					case "keep_none":
					case "false":
						 return ThresholdConfigValue.KeepLastFile;
					default:
						 throw new System.ArgumentException( "Invalid log pruning configuration value '" + configValue + "'. The form is 'true', 'false' or '<number><unit> <type>'. For example, '100k txs' " + "will keep the 100 000 latest transactions." );
					}
			  }
			  else
			  {
					long thresholdValue = parseLongWithUnit( boolOrNumber );
					string thresholdType = tokens[1];
					return new ThresholdConfigValue( thresholdType, thresholdValue );
			  }
		 }
	}

}