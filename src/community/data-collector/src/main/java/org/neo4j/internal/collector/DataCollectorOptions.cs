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
namespace Neo4Net.Internal.Collector
{

	using InvalidArgumentsException = Neo4Net.Kernel.Api.Exceptions.InvalidArgumentsException;

	/// <summary>
	/// Helper classes which are used to define options to data collector procedures.
	/// </summary>
	internal class DataCollectorOptions
	{
		 private DataCollectorOptions()
		 {
		 }

		 internal abstract class Option<T>
		 {
			  internal readonly string Name;
			  internal readonly T DefaultValue;

			  internal Option( string name, T defaultValue )
			  {
					this.Name = name;
					this.DefaultValue = defaultValue;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract T parse(Object value) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException;
			  internal abstract T Parse( object value );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T parseOrDefault(java.util.Map<String,Object> valueMap) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException
			  internal virtual T ParseOrDefault( IDictionary<string, object> valueMap )
			  {
					if ( valueMap.ContainsKey( Name ) )
					{
						 object o = valueMap[Name];
						 return Parse( o );
					}
					return DefaultValue;
			  }
		 }

		 internal class IntOption : Option<int>
		 {
			  internal IntOption( string name, int defaultValue ) : base( name, defaultValue )
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: Integer parse(Object value) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException
			  internal override Integer Parse( object value )
			  {
					int x = AsInteger( value );
					if ( x < 0 )
					{
						 throw new InvalidArgumentsException( string.Format( "Option `{0}` requires positive integer argument, got `{1:D}`", Name, x ) );
					}
					return x;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int asInteger(Object value) throws Neo4Net.kernel.api.exceptions.InvalidArgumentsException
			  internal virtual int AsInteger( object value )
			  {
					if ( value is sbyte? || value is short? || value is int? || value is long? )
					{
						 return ( ( Number )value ).intValue();
					}
					throw new InvalidArgumentsException( string.Format( "Option `{0}` requires integer argument, got `{1}`", Name, value ) );
			  }
		 }
	}

}