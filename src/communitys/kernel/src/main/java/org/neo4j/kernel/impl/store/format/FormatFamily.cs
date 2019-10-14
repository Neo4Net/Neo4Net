using System;

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
namespace Neo4Net.Kernel.impl.store.format
{
	/// <summary>
	/// Family of the format. Family of format is specific to a format across all version that format support.
	/// Two formats in different versions should have same format family.
	/// Family is one of the criteria that will determine if migration between formats is possible.
	/// </summary>
	public abstract class FormatFamily : IComparable<FormatFamily>
	{
		 /// <summary>
		 /// Get format family name </summary>
		 /// <returns> family name </returns>
		 public abstract string Name { get; }

		 /// <summary>
		 /// get format family rank </summary>
		 /// <returns> rank </returns>
		 public abstract int Rank();

		 public override int CompareTo( FormatFamily formatFamily )
		 {
			  return Integer.compare( this.Rank(), formatFamily.Rank() );
		 }

		 /// <summary>
		 /// Check if new record format family is higher then old record format family.
		 /// New format family is higher then old one in case when it safe to migrate from old format into new
		 /// format in terms of format capabilities and determined by family rank: new family is higher if it's rank is higher
		 /// then rank
		 /// of old family </summary>
		 /// <param name="newFormat"> new record format </param>
		 /// <param name="oldFormat"> old record format </param>
		 /// <returns> true if new record format family is higher </returns>
		 public static bool IsHigherFamilyFormat( RecordFormats newFormat, RecordFormats oldFormat )
		 {
			  return oldFormat.FormatFamily.CompareTo( newFormat.FormatFamily ) < 0;
		 }

		 /// <summary>
		 /// Check if record formats have same format family </summary>
		 /// <param name="recordFormats1"> first record format </param>
		 /// <param name="recordFormats2"> second record format </param>
		 /// <returns> true if formats have the same format family </returns>
		 public static bool IsSameFamily( RecordFormats recordFormats1, RecordFormats recordFormats2 )
		 {
			  return recordFormats1.FormatFamily.Equals( recordFormats2.FormatFamily );
		 }

		 /// <summary>
		 /// Check if new record format family is lower then old record format family.
		 /// New format family is lower then old one in case when its not safe to migrate from old format into new
		 /// format in terms of format capabilities and determined by family rank: new family is lower if it's rank is lower
		 /// then rank of old family </summary>
		 /// <param name="newFormat"> new record format </param>
		 /// <param name="oldFormat"> old record format </param>
		 /// <returns> true if new record format family is lower </returns>
		 public static bool IsLowerFamilyFormat( RecordFormats newFormat, RecordFormats oldFormat )
		 {
			  return oldFormat.FormatFamily.CompareTo( newFormat.FormatFamily ) > 0;
		 }
	}

}