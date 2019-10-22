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
namespace Neo4Net.Storageengine.Api.schema
{

	using IndexPopulationProgress = Neo4Net.GraphDb.index.IndexPopulationProgress;
	using Neo4Net.Helpers.Collections;

	public interface PopulationProgress
	{

		 long Completed { get; }

		 long Total { get; }

		 float Progress { get; }

		 IndexPopulationProgress ToIndexPopulationProgress();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static PopulationProgress single(long completed, long total)
	//	 {
	//		  return new PopulationProgress()
	//		  {
	//				@@Override public long getCompleted()
	//				{
	//					 return completed;
	//				}
	//
	//				@@Override public long getTotal()
	//				{
	//					 return total;
	//				}
	//
	//				@@Override public float getProgress()
	//				{
	//					 return (total == 0) ? 0 : (float)((double) completed / total);
	//				}
	//
	//				@@Override public IndexPopulationProgress toIndexPopulationProgress()
	//				{
	//					 return new IndexPopulationProgress(completed, total);
	//				}
	//
	//				@@Override public String toString()
	//				{
	//					 return format("[%d/%d:%f]", completed, total, getProgress());
	//				}
	//		  };
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static PopulationProgress_MultiBuilder multiple()
	//	 {
	//		  return new MultiBuilder();
	//	 }
	}

	public static class PopulationProgress_Fields
	{
		 public static readonly PopulationProgress None = single( 0, 0 );
		 public static readonly PopulationProgress Done = single( 1, 1 );
	}

	 public class PopulationProgress_MultiBuilder
	 {
		  internal readonly IList<Pair<PopulationProgress, float>> Parts = new List<Pair<PopulationProgress, float>>();
		  internal float TotalWeight;

		  public virtual PopulationProgress_MultiBuilder Add( PopulationProgress part, float weight )
		  {
				Parts.Add( Pair.of( part, weight ) );
				TotalWeight += weight;
				return this;
		  }

		  public virtual PopulationProgress Build()
		  {
				float[] weightFactors = BuildWeightFactors();
				return new PopulationProgressAnonymousInnerClass( this, weightFactors );
		  }

		  private class PopulationProgressAnonymousInnerClass : PopulationProgress
		  {
			  private readonly PopulationProgress_MultiBuilder _outerInstance;

			  private float[] _weightFactors;

			  public PopulationProgressAnonymousInnerClass( PopulationProgress_MultiBuilder outerInstance, float[] weightFactors )
			  {
				  this.outerInstance = outerInstance;
				  this._weightFactors = weightFactors;
			  }

			  public long Completed
			  {
				  get
				  {
						return _outerInstance.parts.Select( part => part.first().Completed ).Sum();
				  }
			  }

			  public long Total
			  {
				  get
				  {
						return _outerInstance.parts.Select( part => part.first().Total ).Sum();
				  }
			  }

			  public float Progress
			  {
				  get
				  {
						float combined = 0;
						for ( int i = 0; i < _outerInstance.parts.Count; i++ )
						{
							 combined += _outerInstance.parts[i].first().Progress * _weightFactors[i];
						}
						return combined;
				  }
			  }

			  public IndexPopulationProgress toIndexPopulationProgress()
			  {
					// Here we want to control the progress percentage and the best way to do that without introducing
					// another IndexPopulationProgress constructor is to make up completed/total values that will generate
					// the progress we want (nobody uses getCompleted()/getTotal() anyway since even the widely used IndexPopulationProgress#DONE)
					// destroys any actual numbers by having 1/1.
					float progress = Progress;
					long fakeTotal = 1_000; // because we have 4 value digits in the report there
					long fakeCompleted = ( long )( ( float ) fakeTotal * progress );
					return new IndexPopulationProgress( fakeCompleted, fakeTotal );
			  }
		  }

		  internal virtual float[] BuildWeightFactors()
		  {
				float[] weightFactors = new float[Parts.Count];
				float weightSum = 0;
				for ( int i = 0; i < Parts.Count; i++ )
				{
					 Pair<PopulationProgress, float> part = Parts[i];
					 weightFactors[i] = i == Parts.Count - 1 ? 1 - weightSum : part.Other() / TotalWeight;
					 weightSum += weightFactors[i];
				}
				return weightFactors;
		  }
	 }

}