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
namespace Neo4Net.Kernel.Impl.Api
{
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Validates <seealso cref="TextValue text values"/> so that they are within a certain length, byte-wise.
	/// </summary>
	public class IndexTextValueLengthValidator : AbstractIndexKeyLengthValidator
	{
		 internal IndexTextValueLengthValidator( int maxByteLength ) : base( maxByteLength )
		 {
		 }

		 protected internal override int IndexKeyLength( Value value )
		 {
			  return ( ( TextValue )value ).stringValue().GetBytes().length;
		 }

		 public virtual void Validate( sbyte[] encodedValue )
		 {
			  if ( encodedValue == null )
			  {
					throw new System.ArgumentException( "Null value" );
			  }
			  ValidateLength( encodedValue.Length );
		 }
	}

}