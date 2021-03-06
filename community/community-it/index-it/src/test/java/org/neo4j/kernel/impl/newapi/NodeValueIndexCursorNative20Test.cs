﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Newapi
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;

	public class NodeValueIndexCursorNative20Test : AbstractNodeValueIndexCursorTest
	{
		 public override ReadTestSupport NewTestSupport()
		 {
			  ReadTestSupport readTestSupport = new ReadTestSupport();
			  readTestSupport.AddSetting( GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName() );
			  return readTestSupport;
		 }

		 protected internal override string ProviderKey()
		 {
			  return "lucene+native";
		 }

		 protected internal override string ProviderVersion()
		 {
			  return "2.0";
		 }

		 protected internal override bool IndexProvidesStringValues()
		 {
			  return true;
		 }

		 protected internal override bool IndexProvidesNumericValues()
		 {
			  return true;
		 }
	}

}