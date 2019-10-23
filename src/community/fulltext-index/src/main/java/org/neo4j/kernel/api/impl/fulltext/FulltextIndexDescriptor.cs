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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;


	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

	internal class FulltextIndexDescriptor : StoreIndexDescriptor
	{
		 private readonly IList<string> _propertyNames;
		 private readonly Analyzer _analyzer;
		 private readonly string _analyzerName;
		 private readonly bool _eventuallyConsistent;

		 internal FulltextIndexDescriptor( StoreIndexDescriptor descriptor, IList<string> propertyNames, Analyzer analyzer, string analyzerName, bool eventuallyConsistent ) : base( descriptor )
		 {
			  this._propertyNames = propertyNames;
			  this._analyzer = analyzer;
			  this._analyzerName = analyzerName;
			  this._eventuallyConsistent = eventuallyConsistent;
		 }

		 internal virtual ICollection<string> PropertyNames()
		 {
			  return _propertyNames;
		 }

		 public virtual Analyzer Analyzer()
		 {
			  return _analyzer;
		 }

		 internal virtual string AnalyzerName()
		 {
			  return _analyzerName;
		 }

		 public override bool EventuallyConsistent
		 {
			 get
			 {
				  return _eventuallyConsistent;
			 }
		 }

		 public override bool FulltextIndex
		 {
			 get
			 {
				  return true;
			 }
		 }
	}

}