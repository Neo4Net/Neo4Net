﻿/*
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
namespace Neo4Net.Kernel.impl.pagecache
{
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class PublishPageCacheTracerMetricsAfterStart : LifecycleAdapter
	{
		 private readonly PageCursorTracerSupplier _pageCursorTracerSupplier;

		 public PublishPageCacheTracerMetricsAfterStart( PageCursorTracerSupplier pageCursorTracerSupplier )
		 {
			  this._pageCursorTracerSupplier = pageCursorTracerSupplier;
		 }

		 public override void Start()
		 {
			  // This will be called in the final stages of starting up a database, and will report any paging tracer
			  // events caused by the startup process in the starting thread.
			  _pageCursorTracerSupplier.get().reportEvents();
		 }
	}

}