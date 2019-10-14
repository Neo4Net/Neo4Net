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
namespace Neo4Net.Io.pagecache.tracing.cursor
{

	/// <summary>
	/// Supplier of page cursor tracers <seealso cref="PageCursorTracer"/> that use to provide transaction local page cache counters.
	/// </summary>
	public interface PageCursorTracerSupplier : System.Func<PageCursorTracer>
	{

	}

	public static class PageCursorTracerSupplier_Fields
	{
		 public static readonly PageCursorTracerSupplier Null = () => PageCursorTracer.NULL;
	}

}