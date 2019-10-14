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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Neo4Net.Index.Internal.gbptree;

	/// <summary>
	/// <seealso cref="Layout"/> for numbers where numbers need to be unique.
	/// </summary>
	internal class NumberLayoutUnique : NumberLayout
	{
		 private const string IDENTIFIER_NAME = "UNI";
		 private const int MAJOR_VERSION = 0;
		 private const int MINOR_VERSION = 1;

		 internal NumberLayoutUnique() : base(Layout.namedIdentifier(IDENTIFIER_NAME, NumberIndexKey.Size), MAJOR_VERSION, MINOR_VERSION)
		 {
		 }
	}

}