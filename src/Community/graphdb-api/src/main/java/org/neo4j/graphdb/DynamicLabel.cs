using System;

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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// A dynamically instantiated and named <seealso cref="Label"/>. This class is
	/// a convenience implementation of <code>Label</code> that is
	/// typically used when labels are created and named after a
	/// condition that can only be detected at runtime.
	/// 
	/// For statically known labels please consider the enum approach as described
	/// in <seealso cref="Label"/> documentation.
	/// </summary>
	/// <seealso cref= Label </seealso>
	/// @deprecated use <seealso cref="Label.label(string)"/> instead 
	[Obsolete("use <seealso cref=\"Label.label(string)\"/> instead")]
	public class DynamicLabel : Label
	{
		 /// <param name="labelName"> the name of the label. </param>
		 /// <returns> a <seealso cref="Label"/> instance for the given {@code labelName}. </returns>
		 /// @deprecated use <seealso cref="Label.label(string)"/> instead 
		 [Obsolete("use <seealso cref=\"Label.label(string)\"/> instead")]
		 public static Label Label( string labelName )
		 {
			  return new DynamicLabel( labelName );
		 }

		 private readonly string _name;

		 private DynamicLabel( string labelName )
		 {
			  this._name = labelName;
		 }

		 public override string Name()
		 {
			  return this._name;
		 }

		 public override string ToString()
		 {
			  return this._name;
		 }

		 public override bool Equals( object other )
		 {
			  return other is Label && ( ( Label ) other ).Name().Equals(_name);
		 }

		 public override int GetHashCode()
		 {
			  return 26578 ^ _name.GetHashCode();
		 }
	}

}