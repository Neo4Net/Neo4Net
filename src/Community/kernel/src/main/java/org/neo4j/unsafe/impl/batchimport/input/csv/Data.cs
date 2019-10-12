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
namespace Neo4Net.@unsafe.Impl.Batchimport.input.csv
{
	using Neo4Net.Collection;
	using CharReadable = Neo4Net.Csv.Reader.CharReadable;
	using CharSeeker = Neo4Net.Csv.Reader.CharSeeker;

	/// <summary>
	/// Produces a <seealso cref="CharSeeker"/> that can seek and extract values from a csv/tsv style data stream.
	/// A decorator also comes with it which can specify global overrides/defaults of extracted input entities.
	/// </summary>
	public interface Data
	{
		 RawIterator<CharReadable, IOException> Stream();

		 Decorator Decorator();
	}

}